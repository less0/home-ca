using System.Runtime.ConstrainedExecution;
using FluentAssertions;
using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using home_ca_backend.Core.CertificateAuthorityServerAggregate.Exceptions;
using Xunit;

namespace homa_ca_backend.Core.Tests;

public class CertificateAuthorityServerTests
{
    [Fact]
    public void GetRootCertificateAuthorities_IsInitializedEmpty()
    {
        var componentUnderTest = new CertificateAuthorityServer();
        var rootCertificateAuthorities = componentUnderTest.GetRootCertificateAuthorities();
        rootCertificateAuthorities.Should().BeEmpty();
    }

    [Fact]
    public void AddRootCertificateAuthority_IsAddedCorrectly()
    {
        var componentUnderTest = new CertificateAuthorityServer();
        componentUnderTest.AddRootCertificateAuthority(new CertificateAuthority
        {
            Name = "First Root CA"
        });

        var rootCertificateAuthorities = componentUnderTest.GetRootCertificateAuthorities();
        rootCertificateAuthorities.Should().HaveCount(1);
        rootCertificateAuthorities.ElementAt(0).Should().Match<CertificateAuthority>(ca => ca.Name == "First Root CA");
    }

    [Fact]
    public void GetIntermediateCertificateAuthorities_ParentDoesNotExist_ThrowsException()
    {
        var componentUnderTest = new CertificateAuthorityServer();
        var exception = Record.Exception(() => componentUnderTest.GetIntermediateCertificateAuthorities(new()));
        exception.Should().BeOfType<Exception>();
    }

    [Fact]
    public void GetIntermediateCertificateAuthorities_ParentExistsAndIsEmpty_ReturnsEmpty()
    {
        var componentUnderTest = new CertificateAuthorityServer();
        var rootCertificateAuthority = new CertificateAuthority()
        {
            Name = "Root"
        };
        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority);
        var intermediateCertificateAuthorities =
            componentUnderTest.GetIntermediateCertificateAuthorities(rootCertificateAuthority.Id);
        intermediateCertificateAuthorities.Should().BeEmpty();
    }

    [Fact]
    public void AddIntermediateCertificateAuthorities_IntermediateCertificateAuthorityIsAdded()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthority rootCertificateAuthority = new()
        {
            Name = "Root"
        };
        CertificateAuthority intermediateCertificateAuthority = new()
        {
            Name = "Intermediate"
        };
        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority);
        componentUnderTest.AddIntermediateCertificateAuthority(rootCertificateAuthority.Id, intermediateCertificateAuthority);
        
        rootCertificateAuthority.IntermediateCertificateAuthorities.Should().HaveCount(1);
        rootCertificateAuthority.IntermediateCertificateAuthorities.First().Id.Should().BeEquivalentTo(intermediateCertificateAuthority.Id);
    }

    [Fact]
    public void AddIntermediateCertificateAuthority_IntermediateCertificateAuthorityIsAddedToCorrectRoot()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthority rootCertificateAuthority = new()
        {
            Name = "Correct Root"
        };
        CertificateAuthority intermediateCertificateAuthority = new()
        {
            Name = "Intermediate"
        };
        componentUnderTest.AddRootCertificateAuthority(new(){ Name = "Root 1"});
        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority);
        componentUnderTest.AddRootCertificateAuthority(new(){ Name = "Root 3"});
        componentUnderTest.AddIntermediateCertificateAuthority(rootCertificateAuthority.Id, intermediateCertificateAuthority);

        rootCertificateAuthority.IntermediateCertificateAuthorities.Should().HaveCount(1);
    }

    [Fact]
    public void AddIntermediateCertificateAuthority_NestedCertificateAuthorityIsAddedCorrectly()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthority rootCertificateAuthority = new()
        {
            Name = "Root"
        };
        CertificateAuthority firstIntermediateCertificateAuthority = new()
        {
            Name = "First Intermediate"
        };
        CertificateAuthority nestedIntermediateCertificateAuthority = new()
        {
            Name = "Nested Intermediate"
        };
        
        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority);
        componentUnderTest.AddIntermediateCertificateAuthority(rootCertificateAuthority.Id, firstIntermediateCertificateAuthority);
        componentUnderTest.AddIntermediateCertificateAuthority(firstIntermediateCertificateAuthority.Id, nestedIntermediateCertificateAuthority);

        firstIntermediateCertificateAuthority.IntermediateCertificateAuthorities.Should().HaveCount(1);
        firstIntermediateCertificateAuthority.IntermediateCertificateAuthorities.First().Id.Should()
            .BeEquivalentTo(nestedIntermediateCertificateAuthority.Id);
    }

    [Fact]
    public void AddRootCertificateAuthority_IdExistsInAnotherRootCertificateAuthority_ThrowsException()
    {
        CertificateAuthorityServer componentUnderTest = new();
        var id = Guid.NewGuid();
        componentUnderTest.AddRootCertificateAuthority(new()
        {
            Name = "Root",
            Id = new(id)
        });
        var exception = Record.Exception(() => componentUnderTest.AddRootCertificateAuthority(new()
        {
            Name = "Duplicate Id Root",
            Id = new(id)
        }));
        exception.Should().BeOfType<DuplicateCertificateAuthorityIdException>();
    }

    [Fact]
    public void AddRootCertificateAuthority_IdExistsInIntermediateCertificateAuthority_ThrowsException()
    {
        CertificateAuthorityServer componentUnderTest = new();
        var rootId = Guid.NewGuid();
        var duplicateId = Guid.NewGuid();
        componentUnderTest.AddRootCertificateAuthority(new()
        {
            Name = "Root",
            Id = new(rootId)
        });
        componentUnderTest.AddIntermediateCertificateAuthority(new(rootId), new()
        {
            Name = "Intermediate",
            Id = new(duplicateId)
        });

        var exception = Record.Exception(() => componentUnderTest.AddRootCertificateAuthority(new()
        {
            Name = "Duplicate Id Root",
            Id = new(duplicateId)
        }));

        exception.Should().BeOfType<DuplicateCertificateAuthorityIdException>();
    }

    [Fact]
    public void AddIntermediateCertificateAuthority_IdExistsInRootCertificateAuthority_ThrowsException()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthorityId firstRootId = new(Guid.NewGuid());
        CertificateAuthorityId secondRootId = new(Guid.NewGuid());
        
        componentUnderTest.AddRootCertificateAuthority(new()
        {
            Name = "Root 1",
            Id = firstRootId
        });
        componentUnderTest.AddRootCertificateAuthority(new()
        {
            Name = "Root 2",
            Id = secondRootId
        });
        var exception = Record.Exception(() => componentUnderTest.AddIntermediateCertificateAuthority(secondRootId,
            new()
            {
                Name = "Intermediate Duplicate Id",
                Id = secondRootId
            }));
        exception.Should().BeOfType<DuplicateCertificateAuthorityIdException>();
    }

    [Fact]
    public void AddIntermediateCertificateAuthority_IdExistsInAnotherRootCertificateAuthority_ThrowsException()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthorityId firstRootId = new(Guid.NewGuid());
        CertificateAuthorityId secondRootId = new(Guid.NewGuid());
        CertificateAuthorityId intermediateId = new(Guid.NewGuid());
        
        componentUnderTest.AddRootCertificateAuthority(new()
        {
            Name = "Root 1",
            Id = firstRootId
        });
        componentUnderTest.AddRootCertificateAuthority(new()
        {
            Name = "Root 2",
            Id = secondRootId
        });
        componentUnderTest.AddIntermediateCertificateAuthority(firstRootId, new()
        {
            Name = "Intermediate 1",
            Id = intermediateId
        });

        var exception = Record.Exception(() => componentUnderTest.AddIntermediateCertificateAuthority(secondRootId,
            new()
            {
                Name = "Intermediate 2",
                Id = intermediateId
            }));

        exception.Should().BeOfType<DuplicateCertificateAuthorityIdException>();
    }
}