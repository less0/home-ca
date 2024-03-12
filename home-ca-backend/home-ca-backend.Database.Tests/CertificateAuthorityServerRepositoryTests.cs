using FluentAssertions;
using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using Xunit.Extensions.AssemblyFixture;

namespace home_ca_backend.Database.Tests;

public class CertificateAuthorityServerRepositoryTests : IAssemblyFixture<DockerDatabaseFixture>
{
    private readonly RawDatabaseAccess _rawDatabaseAccess = new();
   
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

        _rawDatabaseAccess.GetReferenceValueByTableAndId<string>("CertificateAuthority", certificateAuthority.Id.Guid, "Name")
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

        _rawDatabaseAccess.GetValueByTableAndId<Guid>("CertificateAuthority", rootCertificateAuthority.Id.Guid,
                "CertificateAuthorityId")
            .Should().Be(null);
    }
}