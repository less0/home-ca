using FluentValidation;
using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using JetBrains.Annotations;
using MediatR;

namespace home_ca_backend.Application.AddRootCertificateAuthority;

[UsedImplicitly]
public class AddRootCertificateAuthorityHandler(ICertificateAuthorityServerRepository repository)
    : IRequestHandler<AddRootCertificateAuthorityCommand, Response>
{
    public Task<Response> Handle(AddRootCertificateAuthorityCommand request, CancellationToken cancellationToken)
    {
        Validator validator = new();
        validator.ValidateAndThrow(request);

        var certificateAuthorityServer = repository.Load();
        CertificateAuthority certificateAuthority = new()
        {
            Name = request.CertificateAuthority.Name
        };
        certificateAuthorityServer.AddRootCertificateAuthority(certificateAuthority);
        certificateAuthorityServer.GenerateRootCertificate(certificateAuthority.Id, request.Password);
        repository.Save(certificateAuthorityServer);
        
        return Task.FromResult(Response.Valid(certificateAuthority.Id));
    }
}