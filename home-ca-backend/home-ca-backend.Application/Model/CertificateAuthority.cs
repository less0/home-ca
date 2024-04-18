namespace home_ca_backend.Application.Model;

public class CertificateAuthority
{
    public required string? Id { get; set; }
    public required string Name { get; set; }
    public bool IsRoot { get; set; }
}