using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using home_ca_backend.Core.CertificateAuthorityServerAggregate.Exceptions;
using Xunit;

namespace homa_ca_backend.Core.Tests;

public partial class CertificateAuthorityServerTests
{
    [Fact]
    public void GenerateRootCertificateAuthorityCertificate_CertificateIsGenerated()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthority rootCertificateAuthority = new()
        {
            Name = "Root Certificate Authority"
        };
        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority);
        componentUnderTest.GenerateRootCertificate(rootCertificateAuthority.Id, "password");

        rootCertificateAuthority.EncryptedCertificate.Should().NotBeNull();
        X509Certificate2 certificate = new(rootCertificateAuthority.EncryptedCertificate!, "password");
        certificate.SubjectName.Name.Should().Be("CN=Root Certificate Authority");
    }

    [Fact]
    public void GenerateIntermediateCertificateAuthorityCertificate_CertificateIsGenerated()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthority rootCertificateAuthority = new()
        {
            Name = "Root Certificate Authority"
        };
        CertificateAuthority intermediateCertificateAuthority = new()
        {
            Name = "Intermediate Certificate Authority"
        };
        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority);
        componentUnderTest.AddIntermediateCertificateAuthority(rootCertificateAuthority.Id,
            intermediateCertificateAuthority);
        componentUnderTest.GenerateRootCertificate(rootCertificateAuthority.Id, "root password");
        componentUnderTest.GenerateIntermediateCertificate(intermediateCertificateAuthority.Id,
            "intermediate password", "root password");

        intermediateCertificateAuthority.EncryptedCertificate.Should().NotBeNull();
        X509Certificate2 loadedCertificate =
            new(intermediateCertificateAuthority.EncryptedCertificate!, "intermediate password");
        loadedCertificate.SubjectName.Name.Should().Be("CN=Intermediate Certificate Authority");
    }

    [Fact]
    public void GenerateIntermediateCertificateAuthorityCertificate_IdDoesNotExist_ThrowsUnknownCertificateAuthorityIdException()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthorityId nonExistingId = new();
        
        componentUnderTest.AddRootCertificateAuthority(new()
        {
            Name = "Root Certificate Authority"
        });
        componentUnderTest
            .Invoking(x => x.GenerateIntermediateCertificate(nonExistingId, "intermediate password", "root password"))
            .Should()
            .Throw<UnknownCertificateAuthorityIdException>();
    }

    [Fact]
    public void GenerateIntermediateCertificateAuthorityCertificate_IdBelongsToRootCertificate_ThrowsException()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthority rootCertificateAuthority = new()
        {
            Name = "Root Certificate Authority"
        };
        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority);
        componentUnderTest.Invoking(x => x.GenerateIntermediateCertificate(rootCertificateAuthority.Id, "1234", "4321"))
            .Should()
            .Throw<NoIntermediateCertificateAuthorityException>();
    }

    [Fact]
    public void GenerateLeafCertificate_LeafIdDoesNotExist_ThrowsException()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthorityId secondRootId = new();
        
        componentUnderTest.AddRootCertificateAuthority(new()
        {
            Name = "Root Certificate Authority"
        });
        componentUnderTest.AddRootCertificateAuthority(new()
        {
            Id = secondRootId,
            Name = "Second Root Certificate Authority"
        });
        componentUnderTest.AddIntermediateCertificateAuthority(secondRootId, new()
        {
            Name = "Intermediate Certificate Authority"
        });
        componentUnderTest.Invoking(x => x.GenerateLeafCertificate(new(), "leaf password", "certificate authority password"))
            .Should()
            .Throw<UnknownLeafIdException>();
    }

    [Fact]
    public void GenerateLeafCertificate_CertificateIsGenerated()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthority rootCertificateAuthority = new()
        {
            Name = "Root Certificate Authority"
        };
        Leaf leaf = new()
        {
            Name = "foo.local"
        };
        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority);
        componentUnderTest.AddLeaf(rootCertificateAuthority.Id, leaf);
        componentUnderTest.GenerateRootCertificate(rootCertificateAuthority.Id, "root password");
        
        componentUnderTest.GenerateLeafCertificate(leaf.Id, "leaf password", "root password");
        
        leaf.EncryptedCertificate.Should().NotBeNull();
        X509Certificate2 loadedCertificate = new(leaf.EncryptedCertificate!, "leaf password");
        loadedCertificate.SubjectName.Name.Should().Be("CN=foo.local");
    }

    [Fact]
    public void GenerateRootCertificate_PemCertificateIsSet()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthority rootCertificateAuthority = new() { Name = "Root Certificate Authority" };
        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority);
        componentUnderTest.GenerateRootCertificate(rootCertificateAuthority.Id, "root password");
        rootCertificateAuthority.PemCertificate.Should().NotBeNull();
    }

    [Fact]
    public void GenerateIntermediateCertificate_PemCertificateIsSet()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthority rootCertificateAuthority = new()
        {
            Name = "Root Certificate Authority"
        };
        CertificateAuthority intermediateCertificateAuthority = new() { Name = "Intermediate Certificate Authority" };
        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority);
        componentUnderTest.AddIntermediateCertificateAuthority(rootCertificateAuthority.Id,
            intermediateCertificateAuthority);
        componentUnderTest.GenerateRootCertificate(rootCertificateAuthority.Id, "root password");
        componentUnderTest.GenerateIntermediateCertificate(intermediateCertificateAuthority.Id, "intermediate password",
            "root password");
        
        intermediateCertificateAuthority.PemCertificate.Should().NotBeNull();
    }
}