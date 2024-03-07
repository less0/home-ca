using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using home_ca_backend.Core.CertificateAuthorityServerAggregate.Exceptions;

namespace home_ca_backend.Core.CertificateAuthorityServerAggregate;

public class CertificateAuthority
{
    private readonly List<CertificateAuthority> _intermediateCertificateAuthorities = new();
    private readonly List<Leaf> _leaves = new();
    
    public CertificateAuthorityId Id { get; init; } = new();
    public required string Name { get; init; }

    public IReadOnlyCollection<CertificateAuthority> IntermediateCertificateAuthorities =>
        new ReadOnlyCollection<CertificateAuthority>(_intermediateCertificateAuthorities);

    public IReadOnlyCollection<Leaf> Leaves => new ReadOnlyCollection<Leaf>(_leaves);
    public string? PemCertificate { get; private set; }
    
    public byte[]? EncryptedCertificate { get; private set; }

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
        using var rsa = RSA.Create(4096);
        var certificateRequest =
            new CertificateRequest("cn=" + Name, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        certificateRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(certificateAuthority: true, hasPathLengthConstraint: false, pathLengthConstraint: 0, critical: true));
        
        var certificate =
            certificateRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(10));
        PemCertificate = certificate.ExportCertificatePem();

        EncryptedCertificate = certificate.Export(X509ContentType.Pfx, password);
    }

    internal void GenerateIntermediateCertificate(CertificateAuthorityId intermediateCertificateAuthorityId, string intermediatePassword, string signingCertificatePassword)
    {
        var intermediateCertificateAuthority =
            _intermediateCertificateAuthorities.First(ca => ca.Id.Equals(intermediateCertificateAuthorityId));
        X509Certificate2 signingCertificate = new(EncryptedCertificate, signingCertificatePassword);
        
        intermediateCertificateAuthority.GenerateSignedCertificate(intermediatePassword, signingCertificate);
    }

    public void GenerateLeafCertificate(LeafId id, string leafPassword, string signingCertificatePassword)
    {
        var leaf = _leaves.First(leaf => leaf.Id.Equals(id));
        X509Certificate2 signingCertificate = new(EncryptedCertificate, signingCertificatePassword);

        leaf.GenerateSignedCertificate(leafPassword, signingCertificate);
    }

    private void GenerateSignedCertificate(string intermediatePassword, X509Certificate2 signingCertificate)
    {
        using var rsa = RSA.Create(4096);
        CertificateRequest request = new($"cn={Name}", rsa, HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(certificateAuthority: true, hasPathLengthConstraint: false, pathLengthConstraint: 0, critical: true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.KeyCertSign, critical: false));
        var certificate = request.Create(signingCertificate, DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(2), SerialNumberGenerator.GenerateSerialNumber());
        certificate = certificate.CopyWithPrivateKey(rsa);
        EncryptedCertificate = certificate.Export(X509ContentType.Pfx, intermediatePassword);
        PemCertificate = certificate.ExportCertificatePem();
    }

    public string GetCertificateChain(LeafId id)
    {
        if (PemCertificate == null)
        {
            throw new MissingPemCertificateException();
        }

        if (HasLeafWithId(id))
        {
            var leaf = _leaves.First(leaf => leaf.Id.Equals(id));
            if (leaf.PemCertificate == null)
            {
                throw new MissingPemCertificateException();
            }

            return $"{leaf.PemCertificate}\n{PemCertificate}";
        }

        var remainingCertificateChain = _intermediateCertificateAuthorities.FirstOrDefault(ca => ca.IsParentOf(id)).GetCertificateChain(id);
        return $"{remainingCertificateChain}\n{PemCertificate}";
    }

    private bool HasLeafWithId(LeafId id)
    {
        return _leaves.Any(leaf => leaf.Id.Equals(id));
    }

    internal bool IsParentOf(LeafId id)
    {
        return _leaves.Any(leaf => leaf.Id.Equals(id))
               || _intermediateCertificateAuthorities.Any(ca => ca.IsParentOf(id));

    }
}