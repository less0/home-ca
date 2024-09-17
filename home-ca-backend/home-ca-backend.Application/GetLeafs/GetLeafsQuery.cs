using MediatR;

namespace home_ca_backend.Application.GetLeafs;

public record GetLeafsQuery(Guid ParentId) : IRequest<GetLeafsResponse>;