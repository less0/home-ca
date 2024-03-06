using System.Collections.ObjectModel;
using home_ca_backend.Core.CertificateAuthorityServerAggregate.Exceptions;

namespace home_ca_backend.Core.CertificateAuthorityServerAggregate;

public class CertificateAuthorityServer
{
    private readonly List<CertificateAuthority> _rootCertificateAuthorities = new();
    
    public IReadOnlyCollection<CertificateAuthority> GetRootCertificateAuthorities() => new ReadOnlyCollection<CertificateAuthority>(_rootCertificateAuthorities);

    public IReadOnlyCollection<CertificateAuthority> GetIntermediateCertificateAuthorities(CertificateAuthorityId parentId)
    {
        var parent = FindById(parentId);
        return parent.IntermediateCertificateAuthorities;
    }

    public void AddRootCertificateAuthority(CertificateAuthority certificateAuthority)
    {
        EnsureIsValidToBeAdded(certificateAuthority);
        _rootCertificateAuthorities.Add(certificateAuthority);
    }

    public void AddIntermediateCertificateAuthority(CertificateAuthorityId parentId, CertificateAuthority certificateAuthority)
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

    public void SetPublicKey(CertificateAuthorityId id, string publicKey)
    {
        var authority = FindById(id);
        authority.PublicKey = publicKey;
    }

    private CertificateAuthority FindById(CertificateAuthorityId id)
    {
        return FindById(_rootCertificateAuthorities, id) ?? throw new UnknownCertificateAuthorityIdException();
    }

    private CertificateAuthority? FindById(IReadOnlyCollection<CertificateAuthority> certificateAuthorities, CertificateAuthorityId id)
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

    public void GenerateRootCertificateAuthorityCertificate(CertificateAuthorityId id, string password)
    {
        var certificateAuthority = _rootCertificateAuthorities.FirstOrDefault(ca => ca.Id.Equals(id));
        certificateAuthority.GenerateCertificate(password);
    }
}