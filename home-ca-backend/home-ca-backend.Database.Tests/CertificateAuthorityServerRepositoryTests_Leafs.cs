using FluentAssertions;
using home_ca_backend.Core.CertificateAuthorityServerAggregate;

namespace home_ca_backend.Database.Tests;

public partial class CertificateAuthorityServerRepositoryTests
{
    [Fact]
    public void Save_LeafsAreSaved()
    {
        CertificateAuthorityServer server = new();
        CertificateAuthorityId rootId = new();
        LeafId leafId1 = new();
        LeafId leafId2 = new();
        LeafId leafId3 = new();
        
        server.AddRootCertificateAuthority(new()
        {
            Name = "Root CA",
            Id = rootId
        });
        server.AddLeaf(rootId,
            new()
            {
                Id = leafId1,
                Name = "Leaf 1",
            });
        server.AddLeaf(rootId,
            new()
            {
                Id = leafId2,
                Name = "Leaf 2"
            });
        server.AddLeaf(rootId,
            new()
            {
                Id = leafId3,
                Name = "Leaf 3"
            });

        CertificateAuthorityServerRepository componentUnderTest = new(DatabaseContextFactory.Create());
        componentUnderTest.Save(server);

        _rawDatabaseAccess.GetReferenceValueByTableAndId<Leaf, string>(leafId1, "Name")
            .Should().Be("Leaf 1");
        _rawDatabaseAccess.GetReferenceValueByTableAndId<Leaf, string>(leafId2, "Name")
            .Should().Be("Leaf 2");
        _rawDatabaseAccess.GetReferenceValueByTableAndId<Leaf, string>(leafId3, "Name")
            .Should().Be("Leaf 3");
    }
}