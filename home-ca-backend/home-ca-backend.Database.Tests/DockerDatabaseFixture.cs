using System.Diagnostics;
using FluentAssertions;

namespace home_ca_backend.Database.Tests;

public class DockerDatabaseFixture : IDisposable
{
    private string _dockerContainerName = Guid.NewGuid().ToString().Replace("-", "");
    
    public DockerDatabaseFixture()
    {
        var process = Process.Start(new ProcessStartInfo("docker")
        {
            Arguments =
                $"run --env=ACCEPT_EULA=Y --env=MSSQL_SA_PASSWORD=2Pq93JS! --env=MSSQL_PID=Express -p 11433:1433 --name {_dockerContainerName} -d mcr.microsoft.com/mssql/server:2022-latest"
        });
        process.WaitForExit();
        process.ExitCode.Should().Be(0);

        RawDatabaseAccess rawDatabaseAccess = new();
        rawDatabaseAccess.WaitForConnection();
    }

    public void Dispose()
    {
        var process = Process.Start("docker", $"stop {_dockerContainerName}");
        process.WaitForExit();
        process = Process.Start("docker", $"rm {_dockerContainerName}");
        process.WaitForExit();
    }
}