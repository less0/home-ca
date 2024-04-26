using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using CertificateAuthority = home_ca_backend.Application.Model.CertificateAuthority;

namespace home_ca_backend.Application;

public class GetChildrenCertificateAuthoritiesValidResponse(CertificateAuthority[] certificateAuthorities)
    : GetChildrenCertificateAuthoritiesResponse
{
    public CertificateAuthority[] CertificateAuthorities { get; } = certificateAuthorities;

    public override bool IsValid => true;
}