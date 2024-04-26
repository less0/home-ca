using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using home_ca_backend.Core.CertificateAuthorityServerAggregate.Exceptions;
using JetBrains.Annotations;
using MediatR;
using CertificateAuthority = home_ca_backend.Application.Model.CertificateAuthority;

namespace home_ca_backend.Application;

[UsedImplicitly]
public class GetChildrenCertificateAuthoritiesHandler(ICertificateAuthorityServerRepository certificateAuthorityServerRepository)
    : IRequestHandler<GetChildrenCertificateAuthorities, GetChildrenCertificateAuthoritiesResponse>
{
    public Task<GetChildrenCertificateAuthoritiesResponse> Handle(GetChildrenCertificateAuthorities request, CancellationToken cancellationToken)
    {
        try
        {
            var server = certificateAuthorityServerRepository.Load();
            var result = server.GetIntermediateCertificateAuthorities(new(request.Id))
                .Select(x => new CertificateAuthority
                {
                    Id = x.Id.Guid.ToString(),
                    Name = x.Name
                })
                .ToArray();
            return Task.FromResult(GetChildrenCertificateAuthoritiesResponse.CreateValid(result));
        }
        catch (UnknownCertificateAuthorityIdException)
        {
            return Task.FromResult(GetChildrenCertificateAuthoritiesResponse.CreateParentIdNotFoundResponse());
        }
    }
}