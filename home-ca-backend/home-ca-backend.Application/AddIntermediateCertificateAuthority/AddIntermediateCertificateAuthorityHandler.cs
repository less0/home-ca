using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using home_ca_backend.Core.CertificateAuthorityServerAggregate.Exceptions;
using JetBrains.Annotations;
using MediatR;

namespace home_ca_backend.Application.AddIntermediateCertificateAuthority;

[UsedImplicitly]
public class AddIntermediateCertificateAuthorityHandler(ICertificateAuthorityServerRepository repository)
    : IRequestHandler<AddIntermediateCertificateAuthority, IResponse>
{
    public Task<IResponse> Handle(AddIntermediateCertificateAuthority request, CancellationToken cancellationToken)
    {
        try
        {
            var server = repository.Load();
            var certificateAuthority = new CertificateAuthority
            {
                Name = request.CertificateAuthority.Name
            };
            server.AddIntermediateCertificateAuthority(new(new(request.ParentId)), certificateAuthority);
            repository.Save(server);
            return Task.FromResult<IResponse>(new AddIntermediateCertificateValidResponse
            {
                CreatedCertificateAuthorityId = certificateAuthority.Id
            });
        }
        catch (UnknownCertificateAuthorityIdException)
        {
            return Task.FromResult<IResponse>(new AddIntermediateCertificateParentNotFoundResponse());
        }
    }
}