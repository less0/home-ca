using System.Collections.ObjectModel;
using home_ca_backend.Core.CertificateAuthorityServerAggregate.Exceptions;

namespace home_ca_backend.Core.CertificateAuthorityServerAggregate;

public class CertificateAuthorityServer
{
    private readonly List<CertificateAuthority> _rootCertificateAuthorities = new();

    public IReadOnlyCollection<CertificateAuthority> GetRootCertificateAuthorities()
    {
        return new ReadOnlyCollection<CertificateAuthority>(_rootCertificateAuthorities);
    }

    public IReadOnlyCollection<CertificateAuthority> GetIntermediateCertificateAuthorities(
        CertificateAuthorityId parentId)
    {
        var parent = FindById(parentId);
        return parent.IntermediateCertificateAuthorities;
    }

    public void AddRootCertificateAuthority(CertificateAuthority certificateAuthority)
    {
        EnsureIsValidToBeAdded(certificateAuthority);
        _rootCertificateAuthorities.Add(certificateAuthority);
    }

    public void AddIntermediateCertificateAuthority(CertificateAuthorityId parentId,
        CertificateAuthority certificateAuthority)
    {
        EnsureIsValidToBeAdded(certificateAuthority);
        var parent = FindById(parentId);
        parent.AddIntermediateCertificateAuthority(certificateAuthority);
    }

    public void AddLeaf(CertificateAuthorityId id, Leaf leaf)
    {
        EnsureIsValidToBeAdded(leaf);
        var parent = FindById(id);
        parent.AddLeaf(leaf);
    }

    public void GenerateRootCertificate(CertificateAuthorityId id, string password)
    {
        var certificateAuthority = _rootCertificateAuthorities.FirstOrDefault(ca => ca.Id.Equals(id));
        certificateAuthority.GenerateCertificate(password);
    }

    public void GenerateIntermediateCertificate(CertificateAuthorityId id, string intermediatePassword, string password)
    {
        var certificateAuthority = FindParent(id);
        certificateAuthority.GenerateIntermediateCertificate(id, intermediatePassword, password);
    }

    public void GenerateLeafCertificate(LeafId id, string leafPassword, string signingPassword)
    {
        var certificateAuthority = FindParent(id);
        certificateAuthority.GenerateLeafCertificate(id, leafPassword, signingPassword);
    }

    public string GetCertificateChain(LeafId id)
    {
        var rootCertificateAuthority = FindRoot(id);
        return rootCertificateAuthority.GetCertificateChain(id);
        return string.Empty;
    }

    private CertificateAuthority FindById(CertificateAuthorityId id)
    {
        return FindById(_rootCertificateAuthorities, id) ?? throw new UnknownCertificateAuthorityIdException();
    }

    private CertificateAuthority? FindById(IReadOnlyCollection<CertificateAuthority> certificateAuthorities,
        CertificateAuthorityId id)
    {
        var matchingCertificateAuthority = certificateAuthorities.FirstOrDefault(ca => ca.Id.Equals(id));

        return matchingCertificateAuthority
               ?? certificateAuthorities.Select(ca => FindById(ca.IntermediateCertificateAuthorities, id))
                   .FirstOrDefault(ca => ca != null);
    }

    private void EnsureIsValidToBeAdded(CertificateAuthority certificateAuthority)
    {
        if (_rootCertificateAuthorities.Any(ca => IdExistsInTree(ca, certificateAuthority.Id)))
        {
            throw new DuplicateCertificateAuthorityIdException();
        }
    }

    private bool IdExistsInTree(CertificateAuthority certificateAuthority, CertificateAuthorityId id)
    {
        return certificateAuthority.Id.Equals(id)
               || certificateAuthority.IntermediateCertificateAuthorities.Any(ca => IdExistsInTree(ca, id));
    }

    private void EnsureIsValidToBeAdded(Leaf leaf)
    {
        if (_rootCertificateAuthorities.Any(ca => LeafExists(leaf.Id, ca)))
        {
            throw new DuplicateLeafIdException();
        }
    }

    private bool LeafExists(LeafId leafId, CertificateAuthority authority)
    {
        return authority.Leaves.Any(leaf => leaf.Id.Equals(leafId))
               || authority.IntermediateCertificateAuthorities.Any(ca => LeafExists(leafId, ca));
    }

    private CertificateAuthority FindParent(CertificateAuthorityId id)
    {
        if (_rootCertificateAuthorities.Any(ca => ca.Id.Equals(id)))
        {
            throw new NoIntermediateCertificateAuthorityException();
        }

        var parent = _rootCertificateAuthorities
            .Select(ca => FindParent(id, ca))
            .FirstOrDefault(ca => ca != null);
        return parent ?? throw new UnknownCertificateAuthorityIdException();
    }

    private CertificateAuthority? FindParent(CertificateAuthorityId id,
        CertificateAuthority currentCertificateAuthority)
    {
        var matchingCertificateAuthority =
            currentCertificateAuthority.IntermediateCertificateAuthorities.FirstOrDefault(ca => ca.Id.Equals(id));
        if (matchingCertificateAuthority != null)
        {
            return currentCertificateAuthority;
        }

        return currentCertificateAuthority.IntermediateCertificateAuthorities.Select(ca => FindParent(id, ca))
            .FirstOrDefault(ca => ca != null);
    }

    private CertificateAuthority FindParent(LeafId id)
    {
        var parent = _rootCertificateAuthorities.Select(ca => FindParent(id, ca))
            .FirstOrDefault(ca => ca != null);
        return parent ?? throw new UnknownLeafIdException();
    }

    private CertificateAuthority? FindParent(LeafId id, CertificateAuthority currentCertificateAuthority)
    {
        if (currentCertificateAuthority.Leaves.Any(leaf => leaf.Id.Equals(id)))
        {
            return currentCertificateAuthority;
        }

        return currentCertificateAuthority.IntermediateCertificateAuthorities
            .Select(ca => FindParent(id, ca))
            .FirstOrDefault(ca => ca != null);
    }

    private CertificateAuthority FindRoot(LeafId id)
    {
        return _rootCertificateAuthorities.FirstOrDefault(ca => ca.IsParentOf(id))
            ?? throw new UnknownLeafIdException();
    }
}