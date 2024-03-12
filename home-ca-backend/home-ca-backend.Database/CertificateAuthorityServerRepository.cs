namespace home_ca_backend.Database;

public class CertificateAuthorityServerRepository
{
    private CertificateAuthorityContext _certificateAuthorityContext;

    public CertificateAuthorityServerRepository(CertificateAuthorityContext certificateAuthorityContext)
    {
        _certificateAuthorityContext = certificateAuthorityContext;
    }
    
    
}