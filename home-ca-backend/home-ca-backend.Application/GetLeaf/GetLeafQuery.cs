using MediatR;

namespace home_ca_backend.Application.GetLeaf
{
    public record GetLeafQuery(Guid leafId) : IRequest<GetLeafResponse>;
}
