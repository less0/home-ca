using home_ca_backend.Core.CertificateAuthorityServerAggregate;

namespace home_ca_backend.Application.AddRootCertificateAuthority;

public record ValidResponse(CertificateAuthorityId Id) : Response(IsValid: true);
