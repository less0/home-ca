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

    [Fact]
    public void Save_LeafsAreSavedWithTheirCertificate()
    {
        CertificateAuthorityServerRepository componentUnderTest = new(DatabaseContextFactory.Create());
        CertificateAuthorityServer server = new();
        CertificateAuthorityId certificateAuthorityId = new();
        LeafId leafId = new();
        
        server.AddRootCertificateAuthority(new()
        {
            Id = certificateAuthorityId,
            Name = "Root"
        });
        server.AddLeaf(certificateAuthorityId,
            new()
            {
                Id = leafId,
                Name = "Leaf"
            });
        server.GenerateRootCertificate(certificateAuthorityId, "root password");
        server.GenerateLeafCertificate(leafId, "leaf password", "root password");
        
        componentUnderTest.Save(server);

        _rawDatabaseAccess.GetReferenceValueByTableAndId<Leaf, string>(leafId, nameof(Leaf.PemCertificate))
            .Should().StartWith("-----BEGIN CERTIFICATE-----\n");
        _rawDatabaseAccess.GetReferenceValueByTableAndId<Leaf, string>(leafId, nameof(Leaf.PemPrivateKey))
            .Should().StartWith("-----BEGIN ENCRYPTED PRIVATE KEY-----\n");
    }

    [Fact]
    public void Load_LeafsAreLoadedCorrectly()
    {
        var certificateAuthorityId = Guid.NewGuid();
        var leafId = Guid.NewGuid();
        
        _rawDatabaseAccess.CreateRootCertificateAuthority(certificateAuthorityId, "Root CA");
        _rawDatabaseAccess.CreateCertificateForRootCertificateAuthority(certificateAuthorityId);
        _rawDatabaseAccess.CreateLeaf(certificateAuthorityId, leafId, "Leaf");
        _rawDatabaseAccess.CreateCertificateForLeaf(leafId, "012345", "123456");

        CertificateAuthorityServerRepository componentUnderTest = new(DatabaseContextFactory.Create());
        var server = componentUnderTest.Load();

        var leaf = server.GetRootCertificateAuthorities()
            .First()
            .Leafs
            .First();
        leaf.PemCertificate.Should().StartWith("-----BEGIN CERTIFICATE-----\n");
        leaf.PemPrivateKey.Should().StartWith("-----BEGIN ENCRYPTED PRIVATE KEY-----\n");
        leaf.EncryptedCertificate.Should().NotBeNull().And.NotBeEmpty();
    }
}