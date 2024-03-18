namespace home_ca_backend.Core.CertificateAuthorityServerAggregate;

public interface ICertificateAuthorityServerRepository
{
    void Save(CertificateAuthorityServer server);
    CertificateAuthorityServer Load();
}