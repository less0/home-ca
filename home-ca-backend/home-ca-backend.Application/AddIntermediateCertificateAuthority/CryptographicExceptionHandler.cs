using MediatR.Pipeline;
using System.Security.Cryptography;

namespace home_ca_backend.Application.AddIntermediateCertificateAuthority;

public class CryptographicExceptionHandler : IRequestExceptionHandler<AddIntermediateCertificateAuthorityCommand, AddIntermediateCertificateAuthorityResponse, CryptographicException>
{
    public Task Handle(AddIntermediateCertificateAuthorityCommand request, CryptographicException exception, RequestExceptionHandlerState<AddIntermediateCertificateAuthorityResponse> state, CancellationToken cancellationToken)
    {
        state.SetHandled(AddIntermediateCertificateAuthorityResponse.InvalidPassword());
        return Task.CompletedTask;
    }
}
