using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using MediatR;

namespace home_ca_backend.Application.GetLeaf
{
    public class GetLeafHandler(ICertificateAuthorityServerRepository repository) : IRequestHandler<GetLeafQuery, GetLeafResponse>
    {
        public Task<GetLeafResponse> Handle(GetLeafQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(GetLeafResponse.IdNotFound());
        }
    }
}
