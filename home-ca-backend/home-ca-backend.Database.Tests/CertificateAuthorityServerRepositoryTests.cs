using FluentAssertions;
using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using NSubstitute;
using Xunit.Extensions.AssemblyFixture;

namespace home_ca_backend.Database.Tests;

/// <summary>
/// Tests for <see cref="CertificateAuthorityServerRepository"/>.
/// </summary>
public partial class CertificateAuthorityServerRepositoryTests : IAssemblyFixture<DockerDatabaseFixture>
{
    private readonly RawDatabaseAccess _rawDatabaseAccess = new();

    public CertificateAuthorityServerRepositoryTests()
    {
        _rawDatabaseAccess.ClearDatabase();
    }
   
    [Fact]
    public void Save_RootCertificateAuthorityIsSaved()
    {
        CertificateAuthorityServerRepository componentUnderTest = new(DatabaseContextFactory.Create());
        CertificateAuthorityServer server = new CertificateAuthorityServer();
        CertificateAuthority certificateAuthority = new()
        {
            Name = "Root Certificate Authority"
        };
        server.AddRootCertificateAuthority(certificateAuthority);
        componentUnderTest.Save(server);

        _rawDatabaseAccess.GetReferenceValueByTableAndId<CertificateAuthority, string>(certificateAuthority.Id, "Name")
            .Should().Be("Root Certificate Authority");
    }

    [Fact]
    public void Save_RootIsSavedWithoutAParent()
    {
        CertificateAuthorityServerRepository componentUnderTest = new(DatabaseContextFactory.Create());
        CertificateAuthorityServer server = new();
        CertificateAuthority rootCertificateAuthority = new()
        {
            Name = "Root Certificate Authority"
        };
        server.AddRootCertificateAuthority(rootCertificateAuthority);
        componentUnderTest.Save(server);
        
        _rawDatabaseAccess.GetValueByTableAndId<CertificateAuthority, Guid>(rootCertificateAuthority.Id,
                "CertificateAuthorityId")
            .Should().Be(null);
    }

    [Fact]
    public void Save_CertificateAuthoritiesAreStoredWithTheirRespectiveAddedDatetime()
    {
        CertificateAuthorityServerRepository componentUnderTest = new(DatabaseContextFactory.Create());
        CertificateAuthorityServer server = new();
        CertificateAuthorityId certificateAuthorityId = new();
        DateTimeOffset createdAt = DateTimeOffset.FromUnixTimeSeconds(Random.Shared.NextInt64(253402300799));
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(createdAt);
        CertificateAuthority certificateAuthority = new(timeProvider)
        {
            Id = certificateAuthorityId,
            Name = "CA"
        };
        server.AddRootCertificateAuthority(certificateAuthority);
        componentUnderTest.Save(server);

        _rawDatabaseAccess
            .GetValueByTableAndId<CertificateAuthority, DateTimeOffset>(certificateAuthorityId, "CreatedAt")
            .Should().Be(createdAt);
    }

    [Fact]
    public void Save_IntermediateIsSaved()
    {
        CertificateAuthorityServerRepository componentUnderTest = new(DatabaseContextFactory.Create());
        CertificateAuthorityServer server = new();
        CertificateAuthorityId rootCertificateAuthorityId = new();
        CertificateAuthorityId intermediateCertificateAuthorityId = new();
        
        server.AddRootCertificateAuthority(new()
        {
            Id = rootCertificateAuthorityId,
            Name = "Root"
        });
        server.AddIntermediateCertificateAuthority(rootCertificateAuthorityId,
            new()
            {
                Id = intermediateCertificateAuthorityId,
                Name = "Intermediate"
            });
        componentUnderTest.Save(server);

        _rawDatabaseAccess
            .GetReferenceValueByTableAndId<CertificateAuthority, string>(intermediateCertificateAuthorityId, "Name")
            .Should().Be("Intermediate");
    }

    [Fact]
    public void Load_IntermediateCertificateAuthoritiesAreLoaded_Level1()
    {
        CertificateAuthorityServerRepository componentUnderTest = new(DatabaseContextFactory.Create());
        CertificateAuthorityServer server = new();
        CertificateAuthorityId rootId = new();
        
        server.AddRootCertificateAuthority(new()
        {
            Id = rootId,
            Name = "Root"
        });
        server.AddIntermediateCertificateAuthority(rootId, new()
        {
            Name = "Intermediate"
        });
        
        componentUnderTest.Save(server);

        componentUnderTest = new(DatabaseContextFactory.Create());
        CertificateAuthorityServer loadedServer = componentUnderTest.Load();

        loadedServer.GetNestingDepth().Should().Be(1);
    }

    [Fact]
    public void Load_IntermediateCertificateAuthoritiesAreLoaded_Nested()
    {
        _rawDatabaseAccess.CreateNestedCertificateAuthorities(nesting: 3);
        
        CertificateAuthorityServerRepository componentUnderTest = new(DatabaseContextFactory.Create());
        var loadedServer = componentUnderTest.Load();
        loadedServer.GetNestingDepth().Should().Be(3);
    }

    [Fact]
    public void Load_CertificateAuthoritiesAreLoadedWithCertificates()
    {
        // For this test it should be irrelevant whether the certificate text is a real PEM certificate, hence a dummy
        // text is used to simplify the test.
        var id = Guid.NewGuid();
        _rawDatabaseAccess.CreateRootCertificateAuthorityWithCertificate(id, "Root I", "CERTIFICATE");

        CertificateAuthorityServerRepository componentUnderTest = new(DatabaseContextFactory.Create());
        var loadedServer = componentUnderTest.Load();

        loadedServer.GetRootCertificateAuthorities()
            .First(ca => ca.Id.Guid == id)
            .PemCertificate.Should().Be(
                "CERTIFICATE");
    }

    [Fact]
    public void Load_CertificateAuthoritiesAreLoadedWithPrivateKeys()
    {
        // For this test it should be irrelevant whether the private key text is a real PEM private key, hence a dummy
        // text is used to simplify the test.
        var id = Guid.NewGuid();
        _rawDatabaseAccess.CreateRootCertificateAuthorityWithPrivateKey(id, "Root I", "PRIVATE KEY");

        CertificateAuthorityServerRepository componentUnderTest = new(DatabaseContextFactory.Create());
        var loadedServer = componentUnderTest.Load();

        loadedServer.GetRootCertificateAuthorities()
            .First(ca => ca.Id.Guid == id)
            .PemPrivateKey.Should().Be("PRIVATE KEY");
    }
}