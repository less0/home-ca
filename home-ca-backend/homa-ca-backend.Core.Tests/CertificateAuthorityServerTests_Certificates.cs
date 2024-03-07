﻿using System.Security.Cryptography.X509Certificates;
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

    [Fact]
    public void GetCertificateChain_LeafIdDoesNotExist_ThrowsException()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthorityId rootId = new();
        componentUnderTest.AddRootCertificateAuthority(new()
        {
            Id = rootId,
            Name = "Root Certificate Authority"
        });
        componentUnderTest.AddIntermediateCertificateAuthority(rootId, new()
        {
            Name = "Intermediate Certificate Authority"
        });

        componentUnderTest.Invoking(x => x.GetCertificateChain(new()))
            .Should()
            .Throw<UnknownLeafIdException>();
    }

    [Fact]
    public void GetCertificateChain_RootCertificateDoesNotExist_MissingPrivateKeyException()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthorityId rootId = new();
        CertificateAuthorityId intermediateId = new();
        LeafId leafId = new();
        
        componentUnderTest.AddRootCertificateAuthority(new()
        {
            Id = rootId,
            Name = "Root Certificate Authority"
        });
        componentUnderTest.AddIntermediateCertificateAuthority(rootId, new()
        {
            Id = intermediateId,
            Name = "Intermediate Certificate Authority"
        });
        componentUnderTest.AddLeaf(intermediateId, new()
        {
            Id = leafId,
            Name = "docker.local"
        });

        componentUnderTest.Invoking(x => x.GetCertificateChain(leafId))
            .Should()
            .Throw<MissingPemCertificateException>();
    }

    [Fact]
    public void GetCertificateChain_IntermediateCertificateMissing_ThrowsException()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthorityId rootId = new();
        CertificateAuthorityId intermediateId = new();
        LeafId leafId = new();
        
        componentUnderTest.AddRootCertificateAuthority(new()
        {
            Name = "Root Certificate Authority",
            Id = rootId,
        });
        componentUnderTest.AddIntermediateCertificateAuthority(rootId, new()
        {
            Id = intermediateId,
            Name = "Intermediate Certificate Authority",
        });
        componentUnderTest.AddLeaf(intermediateId, new Leaf()
        {
            Id = leafId,
            Name = "Leaf Name"
        });
        componentUnderTest.GenerateRootCertificate(rootId, "root password");
        componentUnderTest.Invoking(x => x.GetCertificateChain(leafId))
            .Should()
            .Throw<MissingPemCertificateException>();
    }

    [Fact]
    public void GetCertificateChain_LeafCertificateMissing_ThrowsException()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthorityId rootId = new();
        CertificateAuthorityId intermediateId = new();
        LeafId leafId = new();
        
        componentUnderTest.AddRootCertificateAuthority(new()
        {
            Id = rootId,
            Name = "Root Certificate Authority"
        });
        componentUnderTest.AddIntermediateCertificateAuthority(rootId, new()
        {
            Id = intermediateId,
            Name = "Intermediate Certificate Authority"
        });
        componentUnderTest.AddLeaf(intermediateId, new()
        {
            Name = "docker.local",
            Id = leafId
        });
        
        componentUnderTest.GenerateRootCertificate(rootId, "root password");
        componentUnderTest.GenerateIntermediateCertificate(intermediateId, "intermediate password", "root password");

        componentUnderTest.Invoking(x => x.GetCertificateChain(leafId))
            .Should()
            .Throw<MissingPemCertificateException>();
    }

    [Fact]
    public void GetCertificateChain_ChainIsReturned()
    {
        CertificateAuthorityServer componentUnderTest = new CertificateAuthorityServer();
        CertificateAuthorityId rootId = new();
        CertificateAuthorityId intermediateId = new();
        LeafId leafId = new();
        
        componentUnderTest.AddRootCertificateAuthority(new()
            {
                Id = rootId,
                Name = "Root Certificate Authority"
            });
        componentUnderTest.AddIntermediateCertificateAuthority(rootId,
            new()
            {
                Id = intermediateId,
                Name = "Intermediate Certificate Authority"
            });
        componentUnderTest.AddLeaf(intermediateId, new Leaf
        {
            Name = "Leaf",
            Id = leafId
        });
        
        componentUnderTest.GenerateRootCertificate(rootId, "root password");
        componentUnderTest.GenerateIntermediateCertificate(intermediateId, "intermediate password", "root password");
        componentUnderTest.GenerateLeafCertificate(leafId, "leaf password", "intermediate password");

        var certificateChain = componentUnderTest.GetCertificateChain(leafId);
        certificateChain.Should().NotBeNull().And.NotBeEmpty();
    }

    [Fact]
    public void GetCertificateChain_ContainsCorrectCertificates()
    {
        CertificateAuthorityServer componentUnderTest = new CertificateAuthorityServer();
        CertificateAuthorityId rootId = new();
        CertificateAuthorityId intermediateId = new();
        LeafId leafId = new();
        
        componentUnderTest.AddRootCertificateAuthority(new()
        {
            Id = rootId,
            Name = "Root Certificate Authority"
        });
        componentUnderTest.AddIntermediateCertificateAuthority(rootId,
            new()
            {
                Id = intermediateId,
                Name = "Intermediate Certificate Authority"
            });
        componentUnderTest.AddLeaf(intermediateId, new Leaf
        {
            Name = "Leaf",
            Id = leafId
        });
        
        componentUnderTest.GenerateRootCertificate(rootId, "root password");
        componentUnderTest.GenerateIntermediateCertificate(intermediateId, "intermediate password", "root password");
        componentUnderTest.GenerateLeafCertificate(leafId, "leaf password", "intermediate password");

        var certificateChain = componentUnderTest.GetCertificateChain(leafId);
        var certificates = ReadCertificatesFromPem(certificateChain);

        certificates.Should().HaveCount(3);
        certificates[0].SubjectName.Name.Should().Be("CN=Leaf");
        certificates[1].SubjectName.Name.Should().Be("CN=Intermediate Certificate Authority");
        certificates[2].SubjectName.Name.Should().Be("CN=Root Certificate Authority");
    }
    
    private List<X509Certificate2> ReadCertificatesFromPem(string pemChain)
    {
        var pemCertificates = pemChain.Split("-----END CERTIFICATE-----");

        var x509Certificates = new List<X509Certificate2>();

        foreach (var pemCertificate in pemCertificates)
        {
            if (!string.IsNullOrWhiteSpace(pemCertificate))
            {
                var certificateData = pemCertificate.Replace("-----BEGIN CERTIFICATE-----\n", "");

                X509Certificate2 certificate2 = new X509Certificate2(Convert.FromBase64String(certificateData));
                x509Certificates.Add(certificate2);
            }
        }

        return x509Certificates;
    }
}