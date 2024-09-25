using home_ca_backend.Application.Model;
using home_ca_backend.Core.CertificateAuthorityServerAggregate;

namespace home_ca_backend.Application.AddRootCertificateAuthority;

public record Response(bool IsValid)
{
    public static Response Valid(CertificateAuthorityId id) => new ValidResponse(id);
    public static Response ValidationFailed(params ValidationError[] errors) => new ValidationFailedResponse(errors);
}
