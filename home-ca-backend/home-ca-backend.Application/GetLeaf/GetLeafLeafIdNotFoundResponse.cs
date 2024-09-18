namespace home_ca_backend.Application.GetLeaf
{
    public record GetLeafLeafIdNotFoundResponse : GetLeafResponse
    {
        public GetLeafLeafIdNotFoundResponse()
            : base(false)
        { }
    }
}