using System.Collections.ObjectModel;

namespace home_ca_backend.Api.Model;

public record struct Leaf(string Id, string Name, string? Certificate = null, string? PrivateKey = null)
{
    public static explicit operator Leaf(Application.Model.Leaf leaf) => 
        new(leaf.Id.ToString(), leaf.Name, leaf.Certificate, leaf.PrivateKey);

    public ReadOnlyDictionary<string, string> Links
    {
        get
        {
            Dictionary<string, string> result = new()
            {
                ["self"] = $"/leafs/{Id}"
            };
            return new(result);
        }
    }
}