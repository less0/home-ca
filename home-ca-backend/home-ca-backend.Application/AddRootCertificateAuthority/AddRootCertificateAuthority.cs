using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using MediatR;
using CertificateAuthority = home_ca_backend.Application.Model.CertificateAuthority;

namespace home_ca_backend.Application.AddRootCertificateAuthority;

public class AddRootCertificateAuthority : IRequest<CertificateAuthorityId>
{
    public required CertificateAuthority CertificateAuthority { get; init; }
    
    public required string Password { get; init; }
}