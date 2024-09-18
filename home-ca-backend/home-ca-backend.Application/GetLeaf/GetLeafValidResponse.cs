using home_ca_backend.Application.Model;

namespace home_ca_backend.Application.GetLeaf
{
    public record GetLeafValidResponse : GetLeafResponse
    {
        public Leaf Leaf { get; init; }

        public GetLeafValidResponse(Leaf leaf)
            : base(true)
        {
            Leaf = leaf;
        }
    }
}