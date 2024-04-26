using home_ca_backend.Application.Model;
using MediatR;

namespace home_ca_backend.Application;

public class GetChildrenCertificateAuthorities : IRequest<GetChildrenCertificateAuthoritiesResponse>
{
    public required Guid Id { get; init; }
}