namespace home_ca_backend.Core.CertificateAuthorityServerAggregate;

public class Leaf
{
    public LeafId Id { get; init; } = new();
    public required string Name { get; init; }
}