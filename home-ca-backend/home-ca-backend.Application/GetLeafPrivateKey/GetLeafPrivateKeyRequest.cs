using MediatR;

namespace home_ca_backend.Application.GetLeafPrivateKey;

public record GetLeafPrivateKeyQuery(Guid LeafId) : IRequest<GetLeafPrivateKeyResponse>;
