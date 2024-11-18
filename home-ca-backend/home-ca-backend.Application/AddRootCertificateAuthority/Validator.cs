using FluentValidation;
using static home_ca_backend.Application.Model.PasswordValidator;

namespace home_ca_backend.Application.AddRootCertificateAuthority;

public class Validator : AbstractValidator<AddRootCertificateAuthorityCommand>
{
    public Validator()
    {
        RuleFor(x => x.CertificateAuthority.Name).NotNull()
            .NotEmpty()
            .OverridePropertyName("Name")
            .WithName("Name");
        RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(BeSufficientPassword)
            .WithMessage("Insufficient password strength.");
    }
}