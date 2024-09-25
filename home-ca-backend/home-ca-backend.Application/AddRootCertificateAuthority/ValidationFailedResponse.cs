using home_ca_backend.Application.Model;

namespace home_ca_backend.Application.AddRootCertificateAuthority;

public record ValidationFailedResponse(params ValidationError[] Errors) : Response(IsValid: false);
