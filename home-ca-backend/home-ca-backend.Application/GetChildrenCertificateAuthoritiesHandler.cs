using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using MediatR;
using CertificateAuthority = home_ca_backend.Application.Model.CertificateAuthority;

namespace home_ca_backend.Application;

public class GetChildrenCertificateAuthoritiesHandler(ICertificateAuthorityServerRepository certificateAuthorityServerRepository)
    : IRequestHandler<GetChildrenCertificateAuthorities, CertificateAuthority[]>
{
    public Task<CertificateAuthority[]> Handle(GetChildrenCertificateAuthorities request, CancellationToken cancellationToken)
    {
        var server = certificateAuthorityServerRepository.Load();
        var result = server.GetIntermediateCertificateAuthorities(new(request.Id))
            .Select(x => new CertificateAuthority
            {
                Id = x.Id.Guid.ToString(),
                Name = x.Name
            })
            .ToArray();
        return Task.FromResult(result);
    }
}