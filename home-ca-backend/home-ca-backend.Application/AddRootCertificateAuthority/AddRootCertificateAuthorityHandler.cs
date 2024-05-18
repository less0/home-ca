using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using JetBrains.Annotations;
using MediatR;

namespace home_ca_backend.Application.AddRootCertificateAuthority;

[UsedImplicitly]
public class AddRootCertificateAuthorityHandler(ICertificateAuthorityServerRepository repository)
    : IRequestHandler<AddRootCertificateAuthority, CertificateAuthorityId>
{
    public Task<CertificateAuthorityId> Handle(AddRootCertificateAuthority request, CancellationToken cancellationToken)
    {
        var certificateAuthorityServer = repository.Load();
        var certificateAuthority = new CertificateAuthority
        {
            Name = request.CertificateAuthority.Name
        };
        certificateAuthorityServer.AddRootCertificateAuthority(certificateAuthority);
        certificateAuthorityServer.GenerateRootCertificate(certificateAuthority.Id, request.Password);
        repository.Save(certificateAuthorityServer);
        
        return Task.FromResult(certificateAuthority.Id);
    }
}