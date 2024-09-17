using Leaf = home_ca_backend.Application.Model.Leaf;

namespace home_ca_backend.Application.GetLeafs;

public record GetLeafsValidResponse(IReadOnlyCollection<Leaf> Leafs) : GetLeafsResponse(true);