using MediatR;
using CertificateAuthority = home_ca_backend.Application.Model.CertificateAuthority;

namespace home_ca_backend.Application.AddIntermediateCertificateAuthority;

public class AddIntermediateCertificateAuthority : IRequest<AddIntermediateCertificateAuthorityResponse>
{
    public required CertificateAuthority CertificateAuthority { get; init; }
    public required string ParentId { get; init; }
    public required string Password { get; init; }
    public required string ParentPassword { get; init; }
}