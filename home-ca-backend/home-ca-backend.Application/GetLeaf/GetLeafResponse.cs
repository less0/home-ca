using home_ca_backend.Application.Model;

namespace home_ca_backend.Application.GetLeaf
{
    public record GetLeafResponse(bool IsValid)
    {
        public static GetLeafResponse Valid(Leaf leaf) => new GetLeafValidResponse(leaf);

        public static GetLeafResponse IdNotFound() => new GetLeafLeafIdNotFoundResponse();
    }
}
