using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using home_ca_backend.Core.CertificateAuthorityServerAggregate.Exceptions;

namespace home_ca_backend.Core.CertificateAuthorityServerAggregate;

public class CertificateAuthority
{
    private readonly TimeProvider _timeProvider;
    private readonly List<CertificateAuthority> _intermediateCertificateAuthorities = new();
    private readonly List<Leaf> _leaves = new();
    
    public CertificateAuthority()
        : this(TimeProvider.System)
    {}

    public CertificateAuthority(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }
    
    public CertificateAuthorityId Id { get; init; } = new();
    public required string Name { get; init; }

    public IReadOnlyCollection<CertificateAuthority> IntermediateCertificateAuthorities =>
        new ReadOnlyCollection<CertificateAuthority>(_intermediateCertificateAuthorities);

    public IReadOnlyCollection<Leaf> Leaves => new ReadOnlyCollection<Leaf>(_leaves);
    public string? PemCertificate { get; private set; }
    public byte[]? EncryptedCertificate { get; private set; }
    public string? PemPrivateKey { get; private set; }

    internal void AddIntermediateCertificateAuthority(CertificateAuthority certificateAuthority)
    {
        _intermediateCertificateAuthorities.Add(certificateAuthority);
    }

    internal void AddLeaf(Leaf leaf)
    {
        _leaves.Add(leaf);
    }

    internal void GenerateCertificate(string password)
    {
        var certificate = GenerateSelfSignedCertificate();
        StoreCertificate(certificate, password);
    }

    internal void GenerateIntermediateCertificate(CertificateAuthorityId intermediateCertificateAuthorityId, string intermediatePassword, string signingCertificatePassword)
    {
        var intermediateCertificateAuthority =
            _intermediateCertificateAuthorities.First(ca => ca.Id.Equals(intermediateCertificateAuthorityId));
        X509Certificate2 signingCertificate = GetSigningCertificate(signingCertificatePassword);
        intermediateCertificateAuthority.GenerateSignedCertificate(intermediatePassword, signingCertificate);
    }

    public void GenerateLeafCertificate(LeafId id, string leafPassword, string signingCertificatePassword)
    {
        var leaf = _leaves.First(leaf => leaf.Id.Equals(id));
        X509Certificate2 signingCertificate = GetSigningCertificate(signingCertificatePassword);
        leaf.GenerateSignedCertificate(leafPassword, signingCertificate);
    }

    internal string GetCertificateChain(LeafId id)
    {
        if (PemCertificate == null)
        {
            throw new MissingPemCertificateException();
        }

        if (HasLeafWithId(id))
        {
            var leafPemCertificate = GetLeafPemCertificate(id);
            return $"{leafPemCertificate}\n{PemCertificate}";
        }

        var remainingCertificateChain = GetRemainingCertificateChain(id);
        return $"{remainingCertificateChain}\n{PemCertificate}";
    }

    internal bool IsParentOf(LeafId id)
    {
        return HasLeafWithId(id) 
               || _intermediateCertificateAuthorities.Any(ca => ca.IsParentOf(id));

    }

    private X509Certificate2 GetSigningCertificate(string signingCertificatePassword)
    {
        return new(EncryptedCertificate, signingCertificatePassword);
    }

    private void GenerateSignedCertificate(string intermediatePassword, X509Certificate2 signingCertificate)
    {
        var certificate = GenerateSignedCertificate(signingCertificate);
        StoreCertificate(certificate, intermediatePassword);
    }

    private X509Certificate2 GenerateSelfSignedCertificate()
    {
        using var rsa = RSA.Create(4096);
        var certificateRequest =
            new CertificateRequest("cn=" + Name, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        certificateRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(certificateAuthority: true, hasPathLengthConstraint: false, pathLengthConstraint: 0, critical: true));
        
        var certificate =
            certificateRequest.CreateSelfSigned(_timeProvider.GetUtcNow().AddDays(-1), _timeProvider.GetUtcNow().AddYears(10));
        return certificate;
    }

    private X509Certificate2 GenerateSignedCertificate(X509Certificate2 signingCertificate)
    {
        using var rsa = RSA.Create(4096);
        CertificateRequest request = new($"cn={Name}", rsa, HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(certificateAuthority: true, hasPathLengthConstraint: false, pathLengthConstraint: 0, critical: true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.KeyCertSign, critical: false));

        var notAfter = _timeProvider.GetUtcNow().AddYears(3);
        if (notAfter > signingCertificate.NotAfter)
        {
            notAfter = signingCertificate.NotAfter;
        }
        
        var certificate = request.Create(signingCertificate, _timeProvider.GetUtcNow().AddDays(-1),
            notAfter, SerialNumberGenerator.GenerateSerialNumber());
        certificate = certificate.CopyWithPrivateKey(rsa);
        return certificate;
    }

    private void StoreCertificate(X509Certificate2 certificate, string password)
    {
        EncryptedCertificate = certificate.Export(X509ContentType.Pfx, password);
        PemCertificate = certificate.ExportCertificatePem();
        PemPrivateKey = certificate.ExportEncryptedPkcs8PrivateKeyPem(password);
    }

    private bool HasLeafWithId(LeafId id)
    {
        return _leaves.Any(leaf => leaf.Id.Equals(id));
    }

    private string GetLeafPemCertificate(LeafId id)
    {
        var leaf = _leaves.First(leaf => leaf.Id.Equals(id));
        var leafPemCertificate = leaf.PemCertificate;
        if (leafPemCertificate == null)
        {
            throw new MissingPemCertificateException();
        }

        return leafPemCertificate;
    }

    private string GetRemainingCertificateChain(LeafId id)
    {
        var leafsParent = _intermediateCertificateAuthorities.FirstOrDefault(ca => ca.IsParentOf(id))
            ?? throw new UnknownLeafIdException(); // Should not be thrown anyway
        return leafsParent.GetCertificateChain(id);
    }
}