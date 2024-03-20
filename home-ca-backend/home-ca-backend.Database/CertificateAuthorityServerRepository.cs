using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using Microsoft.EntityFrameworkCore;

namespace home_ca_backend.Database;

public class CertificateAuthorityServerRepository(CertificateAuthorityContext certificateAuthorityContext) : ICertificateAuthorityServerRepository
{
    public void Save(CertificateAuthorityServer server)
    {
        foreach (var certificateAuthority in server.GetRootCertificateAuthorities().Where(IsNewEntry))
        {
            certificateAuthorityContext.Add(certificateAuthority);
        }

        certificateAuthorityContext.SaveChanges();
    }

    public CertificateAuthorityServer Load()
    {
        CertificateAuthorityServer result = new();
        var certificateAuthorities = certificateAuthorityContext.Set<CertificateAuthority>()
            .Where(ca => EF.Property<Guid?>(ca, "CertificateAuthorityId") == null)
            .ToArray();

        foreach (var certificateAuthority in certificateAuthorities)
        {
            LoadContents(certificateAuthority);
            result.AddRootCertificateAuthority(certificateAuthority);
        }

        return result;
    }

    private void LoadContents(CertificateAuthority certificateAuthority)
    {
        certificateAuthorityContext.Entry(certificateAuthority)
            .Collection(ca => ca.Leafs)
            .Load();
        
        certificateAuthorityContext.Entry(certificateAuthority)
            .Collection(ca => ca.IntermediateCertificateAuthorities)
            .Load();
        foreach (var intermediateCertificateAuthority in certificateAuthority.IntermediateCertificateAuthorities)
        {
            LoadContents(intermediateCertificateAuthority);
        }
    }

    private bool IsNewEntry(CertificateAuthority certificateAuthority)
    {
        var entry = certificateAuthorityContext.Entry(certificateAuthority);
        return entry.State == EntityState.Detached;
    }
}