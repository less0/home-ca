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

    private CertificateAuthority FindById(CertificateAuthorityId id)
    {
        // TODO Add custom exception
        return FindById(_rootCertificateAuthorities, id) ?? throw new Exception();
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
}