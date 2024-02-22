using System.Collections.ObjectModel;

namespace home_ca_backend.Api.Model;

public class CertificateAuthority
{
    public string? Id { get; set; }
    
    public required string Name { get; set; }

    public bool IsRoot { get; set; } = false;

    public bool HasChildren { get; set; } = false;

    public List<CertificateAuthority> IntermediateAuthorities { get; } = new();

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