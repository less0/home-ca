namespace home_ca_backend.Application.GetLeaf
{
    public record GetLeafIdNotFoundException : GetLeafResponse
    {
        public GetLeafIdNotFoundException()
            : base(false)
        { }
    }
}