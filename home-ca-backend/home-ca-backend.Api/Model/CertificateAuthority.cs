using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace home_ca_backend.Api.Model;

public record CertificateAuthority(string? Id, string Name, bool IsRoot, bool HasChildren)
{
    [UsedImplicitly]
    public ReadOnlyDictionary<string, string> Links
    {
        get
        {
            Dictionary<string, string> result = new()
            {
                ["self"] = $"/cas/{Id}",
            };

            if (HasChildren)
            {
                result.Add("children", $"/cas/{Id}/children");
            }
            
            return new(result);
        }
    }
}