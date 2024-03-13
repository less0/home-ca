using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using Microsoft.EntityFrameworkCore;

namespace home_ca_backend.Database;

public class CertificateAuthorityServerRepository(CertificateAuthorityContext certificateAuthorityContext)
{
    public void Save(CertificateAuthorityServer server)
    {
        foreach (var certificateAuthority in server.GetRootCertificateAuthorities())
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
            LoadIntermediateRecursively(certificateAuthority);
            result.AddRootCertificateAuthority(certificateAuthority);
        }

        return result;
    }

    private void LoadIntermediateRecursively(CertificateAuthority certificateAuthority)
    {
        certificateAuthorityContext.Entry(certificateAuthority)
            .Collection(ca => ca.IntermediateCertificateAuthorities)
            .Load();
        foreach (var intermediateCertificateAuthority in certificateAuthority.IntermediateCertificateAuthorities)
        {
            LoadIntermediateRecursively(intermediateCertificateAuthority);
        }
    }
}