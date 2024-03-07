using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace home_ca_backend.Core.CertificateAuthorityServerAggregate;

public static class X509Certificate2Extensions
{
    public static string ExportEncryptedPkcs8PrivateKeyPem(this X509Certificate2 certificate, string password)
    {
        PbeParameters pbe = new PbeParameters(PbeEncryptionAlgorithm.Aes128Cbc, HashAlgorithmName.SHA256, 600_000);
        return certificate.GetRSAPrivateKey()!.ExportEncryptedPkcs8PrivateKeyPem(password, pbe);
    }
}