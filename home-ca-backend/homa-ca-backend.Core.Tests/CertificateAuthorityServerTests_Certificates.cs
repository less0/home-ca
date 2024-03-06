using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using Xunit;

namespace homa_ca_backend.Core.Tests;

public partial class CertificateAuthorityServerTests
{
    [Fact]
    public void SetPublicKey_IsSetOnTheCorrectCertificateAuthority()
    {
        CertificateAuthorityServer componentUnderTest = new();

        CertificateAuthorityId firstRootId = new CertificateAuthorityId();
        CertificateAuthority firstRoot = new()
        {
            Id = firstRootId,
            Name = "First Root"
        };

        CertificateAuthorityId secondRootId = new();
        CertificateAuthority secondRoot = new()
        {
            Id = secondRootId,
            Name = "Second Root"
        };
        
        componentUnderTest.AddRootCertificateAuthority(firstRoot);
        componentUnderTest.AddRootCertificateAuthority(secondRoot);

        componentUnderTest.SetPublicKey(firstRootId, "FooBar");

        firstRoot.PublicKey.Should().Be("FooBar");
        secondRoot.PublicKey.Should().BeNull();
    }

    [Fact]
    public void GenerateRootCertificateAuthorityCertificate_CertificateIsGenerated()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthority rootCertificateAuthority = new()
        {
            Name = "Root Certificate Authority"
        };
        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority);
        componentUnderTest.GenerateRootCertificateAuthorityCertificate(rootCertificateAuthority.Id, "password");

        rootCertificateAuthority.EncryptedCertificate.Should().NotBeNull();
        X509Certificate2 certificate = new(rootCertificateAuthority.EncryptedCertificate!, "password");
        certificate.SubjectName.Name.Should().Be("CN=Root Certificate Authority");
    }
}