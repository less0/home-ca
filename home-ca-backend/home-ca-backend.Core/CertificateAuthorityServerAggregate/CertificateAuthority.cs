using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

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
    public string? PublicKey { get; internal set; }
    
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
        PublicKey = certificate.GetPublicKeyString();

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
            DateTimeOffset.UtcNow.AddDays(365), SerialNumberGenerator.GenerateSerialNumber());
        certificate = certificate.CopyWithPrivateKey(rsa);
        EncryptedCertificate = certificate.Export(X509ContentType.Pfx, intermediatePassword);
    }
}