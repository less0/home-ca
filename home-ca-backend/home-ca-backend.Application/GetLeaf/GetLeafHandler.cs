using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using home_ca_backend.Core.CertificateAuthorityServerAggregate.Exceptions;
using JetBrains.Annotations;
using MediatR;

namespace home_ca_backend.Application.GetLeaf
{
    [UsedImplicitly]
    public class GetLeafHandler(ICertificateAuthorityServerRepository repository) : IRequestHandler<GetLeafQuery, GetLeafResponse>
    {
        public Task<GetLeafResponse> Handle(GetLeafQuery request, CancellationToken cancellationToken)
        {
            var certificateAuthorityServer = repository.Load();

            try
            {
                var leaf = certificateAuthorityServer.GetLeaf(new(request.LeafId));
                string? certificateChain = null;
                if (leaf.EncryptedCertificate != null)
                {
                    certificateChain = certificateAuthorityServer.GetCertificateChain(new(request.LeafId));
                }
                return Task.FromResult(GetLeafResponse.Valid(new(leaf.Id.Guid, leaf.Name, certificateChain, null)));
            }
            catch(UnknownLeafIdException)
            {
                return Task.FromResult(GetLeafResponse.IdNotFound());
            }
        }
    }
}
