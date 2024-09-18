using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using MediatR;

namespace home_ca_backend.Application.GetLeafPrivateKey;

public class GetLeafPrivateKeyHandler(ICertificateAuthorityServerRepository certificateAuthorityServerRepository) : IRequestHandler<GetLeafPrivateKeyQuery, GetLeafPrivateKeyResponse>
{
    public Task<GetLeafPrivateKeyResponse> Handle(GetLeafPrivateKeyQuery request, CancellationToken cancellationToken)
    {
        var server = certificateAuthorityServerRepository.Load();
        var leaf = server.GetLeaf(new(request.LeafId));

        if (leaf.PemPrivateKey != null)
        {
            return Task.FromResult(GetLeafPrivateKeyResponse.Valid(leaf.PemPrivateKey));
        }
        return Task.FromResult(GetLeafPrivateKeyResponse.MissingCertificate());
    }
}
