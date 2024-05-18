using MediatR;

namespace home_ca_backend.Application.GetChildrenCertificateAuthorities;

public class GetChildrenCertificateAuthorities : IRequest<GetChildrenCertificateAuthoritiesResponse>
{
    public required Guid Id { get; init; }
}