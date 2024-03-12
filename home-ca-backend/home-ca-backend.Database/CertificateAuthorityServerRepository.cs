using home_ca_backend.Core.CertificateAuthorityServerAggregate;

namespace home_ca_backend.Database;

public class CertificateAuthorityServerRepository
{
    private CertificateAuthorityContext _certificateAuthorityContext;

    public CertificateAuthorityServerRepository(CertificateAuthorityContext certificateAuthorityContext)
    {
        _certificateAuthorityContext = certificateAuthorityContext;
    }

    public void Save(CertificateAuthorityServer server)
    {
        foreach (var certificateAuthority in server.GetRootCertificateAuthorities())
        {
            _certificateAuthorityContext.Add(certificateAuthority);
        }

        _certificateAuthorityContext.SaveChanges();
    }
}