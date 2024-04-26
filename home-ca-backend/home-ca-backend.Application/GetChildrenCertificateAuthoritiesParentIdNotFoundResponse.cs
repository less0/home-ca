namespace home_ca_backend.Application;

public class GetChildrenCertificateAuthoritiesParentIdNotFoundResponse : GetChildrenCertificateAuthoritiesResponse
{
    public override bool IsValid => false;
}