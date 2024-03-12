using Microsoft.Extensions.Configuration;

namespace home_ca_backend.Database.Tests;

public class DatabaseContextFactory
{
    public static CertificateAuthorityContext Create()
    {
        ConfigurationBuilder configurationBuilder = new();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:(Default)"] = Constants.DatabaseConnectionString
        });
        return new(configurationBuilder.Build());
    }
}