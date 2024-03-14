using home_ca_backend.Core.CertificateAuthorityServerAggregate;

namespace home_ca_backend.Database.Tests;

public static class CertificateAuthorityServerExtensions
{
    public static int GetNestingDepth(this CertificateAuthorityServer server)
    {
        return server.GetRootCertificateAuthorities().Max(ca => ca.GetNestingDepth());
    }
}