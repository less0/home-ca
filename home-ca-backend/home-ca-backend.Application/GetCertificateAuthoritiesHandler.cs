using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using JetBrains.Annotations;
using MediatR;
using CertificateAuthority = home_ca_backend.Application.Model.CertificateAuthority;

namespace home_ca_backend.Application;

[UsedImplicitly]
public class GetCertificateAuthoritiesHandler(
    ICertificateAuthorityServerRepository certificateAuthorityServerRepository)
    : IRequestHandler<GetCertificateAuthorities, CertificateAuthority[]>
{
    public Task<CertificateAuthority[]> Handle(GetCertificateAuthorities request, CancellationToken cancellationToken)
    {
        var server = certificateAuthorityServerRepository.Load();
        var result = server.GetRootCertificateAuthorities()
            .OrderBy(x => x.CreatedAt)
            .Select(x => new CertificateAuthority
            {
                Id = x.Id.Guid.ToString(),
                Name = x.Name
            })
            .ToArray();
        return Task.FromResult(result);
    }
}