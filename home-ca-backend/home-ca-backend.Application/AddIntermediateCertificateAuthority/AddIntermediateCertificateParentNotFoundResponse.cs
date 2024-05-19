namespace home_ca_backend.Application.AddIntermediateCertificateAuthority;

public class AddIntermediateCertificateParentNotFoundResponse : IResponse
{
    public bool IsValid => false;
}