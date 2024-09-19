using home_ca_backend.Core.CertificateAuthorityServerAggregate;

namespace home_ca_backend.Application.AddIntermediateCertificateAuthority;

public record AddIntermediateCertificateAuthorityResponse(bool IsValid)
{
    public static AddIntermediateCertificateAuthorityResponse Valid(CertificateAuthorityId createdCertificateAuthorityId) => new ValidResponse(createdCertificateAuthorityId);
    public static AddIntermediateCertificateAuthorityResponse ParentIdNotFound() => new ParentNotFoundResponse();
    public static AddIntermediateCertificateAuthorityResponse InvalidPassword() => new InvalidPasswordResponse();
}
