namespace home_ca_backend.Api.Model;

public class PostIntermediateCertificateAuthority
{
    public string Name { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ParentPassword { get; set; } = null!;
}
