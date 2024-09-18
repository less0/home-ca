using home_ca_backend.Core.CertificateAuthorityServerAggregate.Exceptions;
using MediatR.Pipeline;

namespace home_ca_backend.Application.GetLeafPrivateKey;

internal class GetLeafPrivateKeyUnknownLeafIdExceptionHandler : IRequestExceptionHandler<GetLeafPrivateKeyQuery, GetLeafPrivateKeyResponse, UnknownLeafIdException>
{
    public Task Handle(GetLeafPrivateKeyQuery request, UnknownLeafIdException exception, RequestExceptionHandlerState<GetLeafPrivateKeyResponse> state, CancellationToken cancellationToken)
    {
        state.SetHandled(GetLeafPrivateKeyResponse.UnknownLeafId());
        return Task.CompletedTask;
    }
}
