using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using MediatR;
using CertificateAuthority = home_ca_backend.Application.Model.CertificateAuthority;

namespace home_ca_backend.Application.AddIntermediateCertificateAuthority;

public class AddIntermediateCertificateAuthority : IRequest<IResponse>
{
    public required CertificateAuthority CertificateAuthority { get; init; }
    public required string ParentId { get; init; }
    public required string Password { get; init; }
}