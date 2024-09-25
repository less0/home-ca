using FluentValidation;
using home_ca_backend.Application.Model;
using MediatR.Pipeline;

namespace home_ca_backend.Application.AddRootCertificateAuthority;

internal class ValidationFailedHandler : IRequestExceptionHandler<AddRootCertificateAuthorityCommand, Response, ValidationException>
{
    public Task Handle(AddRootCertificateAuthorityCommand request, ValidationException exception, RequestExceptionHandlerState<Response> state, CancellationToken cancellationToken)
    {
        ValidationError[] errors = exception.Errors.Select(x => new ValidationError(x.PropertyName, x.ErrorMessage)).ToArray();
        state.SetHandled(Response.ValidationFailed(errors));
        return Task.CompletedTask;
    }
}
