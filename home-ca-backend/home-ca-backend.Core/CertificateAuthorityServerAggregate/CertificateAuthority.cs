using System.Collections.ObjectModel;

namespace home_ca_backend.Core.CertificateAuthorityServerAggregate;

public class CertificateAuthority
{
    private readonly List<CertificateAuthority> _intermediateCertificateAuthorities = new();
    
    public CertificateAuthorityId Id { get; init; } = new();
    public required string Name { get; init; }

    public IReadOnlyCollection<CertificateAuthority> IntermediateCertificateAuthorities =>
        new ReadOnlyCollection<CertificateAuthority>(_intermediateCertificateAuthorities);

    internal void AddIntermediateCertificateAuthority(CertificateAuthority certificateAuthority)
    {
        _intermediateCertificateAuthorities.Add(certificateAuthority);
    }
}