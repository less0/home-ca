using home_ca_backend.Application.Model;
using JetBrains.Annotations;
using MediatR;

namespace home_ca_backend.Application;

[UsedImplicitly]
public class GetCertificateAuthoritiesHandler : IRequestHandler<GetCertificateAuthorities, CertificateAuthority[]>
{
    public Task<CertificateAuthority[]> Handle(GetCertificateAuthorities request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Array.Empty<CertificateAuthority>());
    }
}