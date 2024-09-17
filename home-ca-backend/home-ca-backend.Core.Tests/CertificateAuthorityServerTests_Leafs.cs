using FluentAssertions;
using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using home_ca_backend.Core.CertificateAuthorityServerAggregate.Exceptions;
using System.Net.WebSockets;
using Xunit;

namespace homa_ca_backend.Core.Tests;

public partial class CertificateAuthorityServerTests
{
    [Fact]
    public void AddLeaf_IsAddedToCertificateAuthority()
    {
        CertificateAuthorityServer componentUnderTest = new();
        var rootId = new CertificateAuthorityId();
        var rootCertificateAuthority = new CertificateAuthority
        {
            Name = "Root",
            Id = rootId
        };
        var leaf = new Leaf
        {
            Name = "Leaf"
        };

        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority);
        componentUnderTest.AddLeaf(rootId, leaf);

        rootCertificateAuthority.Leafs.Should().Contain(leaf);
    }

    [Fact]
    public void AddLeaf_LeafIsAddedToIntermediateCertificateAuthority()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthorityId rootId = new();
        CertificateAuthorityId intermediateId = new();

        CertificateAuthority rootCertificateAuthority = new() { Name = "Root", Id = rootId };
        var intermediateCertificateAuthority =
            new CertificateAuthority { Name = "Intermediate", Id = intermediateId };
        var leaf = new Leaf { Name = "Leaf" };
        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority);
        componentUnderTest.AddIntermediateCertificateAuthority(rootId, intermediateCertificateAuthority);

        componentUnderTest.AddLeaf(intermediateId, leaf);

        rootCertificateAuthority.Leafs.Should().NotContain(leaf);
        intermediateCertificateAuthority.Leafs.Should().Contain(leaf);
    }

    [Fact]
    public void AddLeaf_LeafIsAddedToCorrectRootCertificateAuthority()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthorityId rootId1 = new();
        CertificateAuthorityId rootId2 = new();
        var leaf = new Leaf { Name = "Leaf" };

        CertificateAuthority rootCertificateAuthority1 = new() { Name = "Root1", Id = rootId1 };
        CertificateAuthority rootCertificateAuthority2 = new() { Name = "Root2", Id = rootId2 };

        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority1);
        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority2);

        componentUnderTest.AddLeaf(rootId1, leaf);

        rootCertificateAuthority1.Leafs.Should().Contain(leaf);
        rootCertificateAuthority2.Leafs.Should().NotContain(leaf);
    }

    [Fact]
    public void AddLeaf_LeafIsAddedToCorrectIntermediateCertificateAuthority()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthorityId rootId = new();
        CertificateAuthorityId intermediateId1 = new();
        CertificateAuthorityId intermediateId2 = new();
        var leaf = new Leaf { Name = "Leaf" };
        CertificateAuthority rootCertificateAuthority = new() { Name = "Root", Id = rootId };
        CertificateAuthority intermediateCertificateAuthority1 =
            new() { Name = "Intermediate1", Id = intermediateId1 };
        CertificateAuthority intermediateCertificateAuthority2 = new() { Name = "Intermediate2", Id = intermediateId2 };

        componentUnderTest.AddRootCertificateAuthority(rootCertificateAuthority);
        componentUnderTest.AddIntermediateCertificateAuthority(rootId, intermediateCertificateAuthority1);
        componentUnderTest.AddIntermediateCertificateAuthority(intermediateId1, intermediateCertificateAuthority2);
        componentUnderTest.AddLeaf(intermediateId2, leaf);

        rootCertificateAuthority.Leafs.Should().NotContain(leaf);
        intermediateCertificateAuthority1.Leafs.Should().NotContain(leaf);
        intermediateCertificateAuthority2.Leafs.Should().Contain(leaf);
    }

    [Fact]
    public void AddLeaf_ThrowsUnknownCertificateAuthorityIdException_WhenCertificateAuthorityIdIsUnknown()
    {
        CertificateAuthorityServer componentUnderTest = new();
        var leaf = new Leaf { Name = "Leaf" };
        
        componentUnderTest.AddRootCertificateAuthority(new CertificateAuthority { Name = "Root" });
        
        Assert.Throws<UnknownCertificateAuthorityIdException>(() => componentUnderTest.AddLeaf(new CertificateAuthorityId(), leaf));
    }

    [Fact]
    public void AddLeaf_LeafIdExistsInSameRoot_ThrowsDuplicateLeafIdException()
    {
        CertificateAuthorityServer componentUnderTest = new();
        var rootId = new CertificateAuthorityId();
        var root = new CertificateAuthority
        {
            Name = "Root",
            Id = rootId
        };

        var commonLeafId = new LeafId();  // Generate a common Id. Adjust this to your LeafId generation code
        var leaf1 = new Leaf
        {
            Name = "Leaf1",
            Id = commonLeafId  // Assign the common Id
        };
        var leaf2 = new Leaf
        {
            Name = "Leaf2",
            Id = commonLeafId  // Assign the common Id
        };

        componentUnderTest.AddRootCertificateAuthority(root);
        componentUnderTest.AddLeaf(rootId, leaf1);

        Assert.Throws<DuplicateLeafIdException>(() => componentUnderTest.AddLeaf(rootId, leaf2));

        root.Leafs.Should().Contain(leaf1);
        root.Leafs.Should().NotContain(leaf2);
    }

    [Fact]
    public void AddLeaf_LeafIdExistsInAnotherRoot_ThrowsDuplicateLeafIdException()
    {
        // Arrange
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthorityId rootId1 = new();
        CertificateAuthorityId rootId2 = new();
        CertificateAuthority root1 = new()
        {
            Name = "Root1",
            Id = rootId1
        };
        CertificateAuthority root2 = new()
        {
            Name = "Root2",
            Id = rootId2
        };

        LeafId commonLeafId = new();  // Generate a common Id. Adjust this to your LeafId generation code
        Leaf leaf1 = new()
        {
            Name = "Leaf1",
            Id = commonLeafId  // Assign the common Id
        };
        Leaf leaf2 = new()
        {
            Name = "Leaf2",
            Id = commonLeafId  // Assign the common Id
        };

        componentUnderTest.AddRootCertificateAuthority(root1);
        componentUnderTest.AddLeaf(rootId1, leaf1);
        componentUnderTest.AddRootCertificateAuthority(root2);

        // Act and assert
        Assert.Throws<DuplicateLeafIdException>(() => componentUnderTest.AddLeaf(rootId2, leaf2));

        // Note: Assert that Leaf2 was not added if your code supports it
        root2.Leafs.Should().NotContain(leaf2);
    }

    [Fact]
    public void GetLeaf_LeafDoesNotExist_ThrowsUnknownLeafIdException()
    {
        // Arrange
        CertificateAuthorityServer componentUnderTest = new();
        Assert.Throws<UnknownLeafIdException>(() => componentUnderTest.GetLeaf(new()));
    }

    [Fact]
    public void GetLeaf_LeafExistsInRootCertificateAuthority_LeafIsReturned()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthorityId rootId = new();
        LeafId leafId = new();

        Leaf leaf = new()
        {
            Name = "Leaf",
            Id = leafId
        };

        componentUnderTest.AddRootCertificateAuthority(new CertificateAuthority()
        {
            Name = "Root CA 1",
            Id = new CertificateAuthorityId()
        });
        componentUnderTest.AddRootCertificateAuthority(new CertificateAuthority()
        {
            Name = "Root CA 2",
            Id = rootId
        });
        componentUnderTest.AddLeaf(rootId, leaf);

        var result = componentUnderTest.GetLeaf(leafId);

        result.Name.Should().Be(leaf.Name);
        result.Id.Should().Be(leaf.Id);
    }

    [Fact]
    public void GetLeaf_LeafExistsInIntermediateCertificateAuthority_LeafIsReturned()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthorityId rootId = new();
        CertificateAuthorityId intermediateId = new();
        LeafId leafId = new();

        Leaf leaf = new()
        {
            Name = "Leaf",
            Id = leafId
        };

        componentUnderTest.AddRootCertificateAuthority(new()
        {
            Name = "Root CA",
            Id = rootId
        });
        componentUnderTest.AddIntermediateCertificateAuthority(rootId,
            new()
            {
                Name = "Intermediate CA",
                Id = intermediateId
            });
        componentUnderTest.AddLeaf(intermediateId, leaf);

        var result = componentUnderTest.GetLeaf(leafId);

        result.Id.Should().Be(leaf.Id);
        result.Name.Should().Be(leaf.Name);
    }

    [Fact]
    public void GetLeaf_Regression_ExceptionIfTheLeafIdWasNotTheExactSameInstance()
    {
        CertificateAuthorityServer componentUnderTest = new();
        CertificateAuthorityId rootId = new();
        Guid leafId = Guid.NewGuid();

        Leaf leaf = new()
        {
            Name = "Leaf",
            Id = new(leafId)
        };

        componentUnderTest.AddRootCertificateAuthority(new CertificateAuthority()
        {
            Name = "Root CA 1",
            Id = new CertificateAuthorityId()
        });
        componentUnderTest.AddRootCertificateAuthority(new CertificateAuthority()
        {
            Name = "Root CA 2",
            Id = rootId
        });
        componentUnderTest.AddLeaf(rootId, leaf);

        var result = componentUnderTest.GetLeaf(new(leafId));

        result.Name.Should().Be(leaf.Name);
        result.Id.Should().Be(leaf.Id);
    }
}