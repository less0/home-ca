using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using home_ca_backend.Database;

namespace home_ca_backend.Api;

public static class DatabaseServiceCollectionExtensions
{
    public static void AddDatabase(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ICertificateAuthorityServerRepository, CertificateAuthorityServerRepository>();
        serviceCollection.AddScoped<CertificateAuthorityContext>();
    }
}