using System.Collections.ObjectModel;
using home_ca_backend.Application.Model;
using Leaf = home_ca_backend.Application.Model.Leaf;

namespace home_ca_backend.Application.GetLeafs;

public abstract record GetLeafsResponse(bool IsValid) : IResponse
{
    public static GetLeafsResponse Valid(IReadOnlyCollection<Leaf> leafs) =>
        new GetLeafsValidResponse(leafs);

    public static GetLeafsResponse ParentIdNotFound() =>
        new GetLeafsUnknownParentIdResponse();
}