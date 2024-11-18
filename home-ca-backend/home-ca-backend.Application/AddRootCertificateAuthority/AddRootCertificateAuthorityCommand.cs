using MediatR;
using CertificateAuthority = home_ca_backend.Application.Model.CertificateAuthority;

namespace home_ca_backend.Application.AddRootCertificateAuthority;

public class AddRootCertificateAuthorityCommand : IRequest<Response>
{
    public required CertificateAuthority CertificateAuthority { get; init; }
    
    public required string Password { get; init; }
}