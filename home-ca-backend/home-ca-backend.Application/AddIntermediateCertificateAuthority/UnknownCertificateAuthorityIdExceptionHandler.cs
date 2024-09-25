using home_ca_backend.Core.CertificateAuthorityServerAggregate.Exceptions;
using MediatR.Pipeline;

namespace home_ca_backend.Application.AddIntermediateCertificateAuthority;

public class UnknownCertificateAuthorityIdExceptionHandler : IRequestExceptionHandler<AddIntermediateCertificateAuthorityCommand, AddIntermediateCertificateAuthorityResponse, UnknownCertificateAuthorityIdException>
{
    public Task Handle(AddIntermediateCertificateAuthorityCommand request, UnknownCertificateAuthorityIdException exception, RequestExceptionHandlerState<AddIntermediateCertificateAuthorityResponse> state, CancellationToken cancellationToken)
    {
        state.SetHandled(AddIntermediateCertificateAuthorityResponse.ParentIdNotFound());
        return Task.CompletedTask;
    }
}
