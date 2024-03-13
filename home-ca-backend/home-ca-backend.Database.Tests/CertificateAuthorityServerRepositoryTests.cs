using System.Runtime.ConstrainedExecution;
using FluentAssertions;
using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using Xunit.Extensions.AssemblyFixture;

namespace home_ca_backend.Database.Tests;

public class CertificateAuthorityServerRepositoryTests : IAssemblyFixture<DockerDatabaseFixture>
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

        var loadedCertificateAuthorities = loadedServer.GetRootCertificateAuthorities();
        loadedCertificateAuthorities.Should().HaveCount(1);
        loadedCertificateAuthorities.First().IntermediateCertificateAuthorities.Should().HaveCount(1);
    }

    [Fact]
    public void Load_IntermediateCertificateAuthoritiesAreLoaded_Nested()
    {
        CertificateAuthorityServerRepository componentUnderTest = new(DatabaseContextFactory.Create());
        CertificateAuthorityServer server = new();
        CertificateAuthorityId firstRootId = new();
        CertificateAuthorityId secondRootId = new();
        CertificateAuthorityId firstIntermediateId = new();
        CertificateAuthorityId secondIntermediateId = new();
        
        server.AddRootCertificateAuthority(new()
        {
            Id = firstRootId,
            Name = "First root"
        });
        server.AddRootCertificateAuthority(new()
        {
            Id = secondRootId,
            Name = "Second root"
        });
        server.AddIntermediateCertificateAuthority(secondRootId,
            new()
            {
                Id = firstIntermediateId,
                Name = "First intermediate"
            });
        server.AddIntermediateCertificateAuthority(firstIntermediateId,
            new()
            {
                Id = secondIntermediateId,
                Name = "Nested intermediate"
            });
        componentUnderTest.Save(server);

        componentUnderTest = new(DatabaseContextFactory.Create());
        var loadedServer = componentUnderTest.Load();
        var loadedCertificateAuthorities = loadedServer.GetRootCertificateAuthorities();
        loadedCertificateAuthorities.Should().HaveCount(2);
        var nestedIntermediateCertificateAuthority =
            loadedCertificateAuthorities.First(ca => ca.Id.Equals(secondRootId))
                .IntermediateCertificateAuthorities
                .First()
                .IntermediateCertificateAuthorities
                .First();
        nestedIntermediateCertificateAuthority.Id.Should().Be(secondIntermediateId);
    }
}