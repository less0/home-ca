using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using CertificateAuthority = home_ca_backend.Application.Model.CertificateAuthority;

namespace home_ca_backend.Application;

public abstract class GetChildrenCertificateAuthoritiesResponse : IResponse
{
    public abstract bool IsValid { get; }
    
    public static GetChildrenCertificateAuthoritiesResponse CreateValid(CertificateAuthority[] certificateAuthorities) 
        => new GetChildrenCertificateAuthoritiesValidResponse(certificateAuthorities);

    public static GetChildrenCertificateAuthoritiesResponse CreateParentIdNotFoundResponse()
        => new GetChildrenCertificateAuthoritiesParentIdNotFoundResponse();
}