using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace home_ca_backend.Core.CertificateAuthorityServerAggregate;

public class Leaf
{
    private readonly TimeProvider _timeProvider;

    public Leaf()
        : this(TimeProvider.System)
    { }
    
    public Leaf(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public LeafId Id { get; init; } = new();
    public required string Name { get; init; }
    public byte[]? EncryptedCertificate { get; set; }
    public string? PemCertificate { get; private set; }
    public string? PemPrivateKey { get; private set; }

    internal void GenerateSignedCertificate(string password, X509Certificate2 signingCertificate)
    {
        var certificate = GenerateSignedCertificate(signingCertificate);
        StoreCertificate(certificate, password);
    }

    private X509Certificate2 GenerateSignedCertificate(X509Certificate2 signingCertificate)
    {
        using var rsa = RSA.Create(4096);
        CertificateRequest request = new($"cn={Name}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid("1.3.6.1.5.5.7.3.1")], false));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        var notAfter = _timeProvider.GetUtcNow().AddDays(365);
        if (notAfter > signingCertificate.NotAfter)
        {
            notAfter = signingCertificate.NotAfter;
        }
        
        var certificate = request.Create(signingCertificate, _timeProvider.GetUtcNow().AddDays(-1),
                notAfter, SerialNumberGenerator.GenerateSerialNumber())
            .CopyWithPrivateKey(rsa);
        return certificate;
    }

    private void StoreCertificate(X509Certificate2 certificate, string password)
    {
        EncryptedCertificate = certificate.Export(X509ContentType.Pfx, password);
        PemCertificate = certificate.ExportCertificatePem();
        PemPrivateKey = certificate.ExportEncryptedPkcs8PrivateKeyPem(password);
    }
}