using home_ca_backend.Core.CertificateAuthorityServerAggregate;

namespace home_ca_backend.Application.AddIntermediateCertificateAuthority;

public class AddIntermediateCertificateValidResponse : IResponse
{
    public bool IsValid => true;
    public required CertificateAuthorityId CreatedCertificateAuthorityId { get; init; }
}