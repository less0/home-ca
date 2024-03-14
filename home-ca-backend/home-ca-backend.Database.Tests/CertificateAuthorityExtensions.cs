using home_ca_backend.Core.CertificateAuthorityServerAggregate;

namespace home_ca_backend.Database.Tests;

public static class CertificateAuthorityExtensions
{
    public static int GetNestingDepth(this CertificateAuthority certificateAuthority)
    {
        if (certificateAuthority.IntermediateCertificateAuthorities.Count == 0)
        {
            return 0;
        }

        return certificateAuthority.IntermediateCertificateAuthorities.Max(ca => ca.GetNestingDepth()) + 1;
    }
}