using FluentValidation;

namespace home_ca_backend.Application.AddRootCertificateAuthority;

public class Validator : AbstractValidator<AddRootCertificateAuthorityCommand>
{
    public Validator()
    {
        RuleFor(x => x.CertificateAuthority.Name).NotNull().NotEmpty();
    }
}
