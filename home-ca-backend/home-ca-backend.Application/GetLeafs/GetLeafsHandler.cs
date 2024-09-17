using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using home_ca_backend.Core.CertificateAuthorityServerAggregate.Exceptions;
using JetBrains.Annotations;
using MediatR;

namespace home_ca_backend.Application.GetLeafs;

[UsedImplicitly]
public class GetLeafsHandler(ICertificateAuthorityServerRepository repository) : IRequestHandler<GetLeafsQuery, GetLeafsResponse>
{
    public Task<GetLeafsResponse> Handle(GetLeafsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var server = repository.Load();
            var leafs = server.GetLeafs(new(request.ParentId));
            return Task.FromResult(GetLeafsResponse.Valid(leafs.Select(MapWithoutCertificate).ToArray()));
        }
        catch (UnknownCertificateAuthorityIdException)
        {
            return Task.FromResult(GetLeafsResponse.ParentIdNotFound());
        }
    }

    private Model.Leaf MapWithoutCertificate(Leaf leaf) => 
        new(leaf.Id.Guid, leaf.Name, null, null);
}