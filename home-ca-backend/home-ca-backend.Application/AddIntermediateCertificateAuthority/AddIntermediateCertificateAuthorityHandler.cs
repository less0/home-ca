using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using JetBrains.Annotations;
using MediatR;

namespace home_ca_backend.Application.AddIntermediateCertificateAuthority;

[UsedImplicitly]
public class AddIntermediateCertificateAuthorityHandler(ICertificateAuthorityServerRepository repository)
    : IRequestHandler<AddIntermediateCertificateAuthorityCommand, AddIntermediateCertificateAuthorityResponse>
{
    public Task<AddIntermediateCertificateAuthorityResponse> Handle(AddIntermediateCertificateAuthorityCommand request, CancellationToken cancellationToken)
    {
        var server = repository.Load();
        var certificateAuthority = new CertificateAuthority
        {
            Name = request.CertificateAuthority.Name
        };
        server.AddIntermediateCertificateAuthority(new(new(request.ParentId)), certificateAuthority);
        server.GenerateIntermediateCertificate(certificateAuthority.Id, request.Password, request.ParentPassword);
        repository.Save(server);

        return Task.FromResult(AddIntermediateCertificateAuthorityResponse.Valid(certificateAuthority.Id));
    }
}