using System.Diagnostics;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace home_ca_backend.Database.Tests;

// ReSharper disable once ClassNeverInstantiated.Global
public class DockerDatabaseFixture : IDisposable
{
    private readonly string _dockerContainerName = Guid.NewGuid().ToString().Replace("-", "");
    
    public DockerDatabaseFixture()
    {
        var process = Process.Start(new ProcessStartInfo("docker")
        {
            Arguments =
                $"run --env=ACCEPT_EULA=Y --env=MSSQL_SA_PASSWORD=2Pq93JS! --env=MSSQL_PID=Express -p {Constants.Port}:1433 --name {_dockerContainerName} -d mcr.microsoft.com/mssql/server:2022-latest"
        });
        process!.WaitForExit();
        process.ExitCode.Should().Be(0);

        RawDatabaseAccess rawDatabaseAccess = new();
        rawDatabaseAccess.WaitForConnection();

        var databaseContext = DatabaseContextFactory.Create();
        databaseContext.Database.Migrate();
    }

    public void Dispose()
    {
        var process = Process.Start("docker", $"stop {_dockerContainerName}");
        process.WaitForExit();
        process = Process.Start("docker", $"rm {_dockerContainerName}");
        process.WaitForExit();
    }
}