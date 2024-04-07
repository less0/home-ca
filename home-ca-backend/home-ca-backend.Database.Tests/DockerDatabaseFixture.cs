using System.Diagnostics;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace home_ca_backend.Database.Tests;

// ReSharper disable once ClassNeverInstantiated.Global
public class DockerDatabaseFixture : IDisposable
{
    private readonly MsSqlContainer _container;

    public DockerDatabaseFixture()
    {
        MsSqlBuilder containerBuilder = new MsSqlBuilder()
            .WithPassword("2Pq93JS!")
            .WithPortBinding(Constants.Port, 1433)
            .WithEnvironment("MSSQL_PID", "Express");
        _container = containerBuilder.Build();
        _container.StartAsync()
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
        
        var databaseContext = DatabaseContextFactory.Create();
        databaseContext.Database.Migrate();
    }

    public void Dispose()
    {
        _container.StopAsync().GetAwaiter().GetResult();
        _container.DisposeAsync().GetAwaiter().GetResult();
    }
}