using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Testcontainers.MsSql;

namespace home_ca_backend.Api.Tests.Drivers
{
    public class Driver
    {
        private static Driver? _instance;
        
        public static Driver Instance => _instance ??= new();
        
        private static readonly int SqlPort = 50000 + Random.Shared.Next(9999);
        private static readonly int KestrelPort = 40000 + Random.Shared.Next(9999);
        
        private MsSqlContainer? _container;
        private Process? _apiProcess;
        
        public HttpClient? HttpClient { get; private set; }

        public HttpStatusCode LastStatusCode { get; set; }

        public async Task StartDatabaseAsync()
        {
            MsSqlBuilder containerBuilder = new MsSqlBuilder()
                .WithPassword("2Pq93JS!")
                .WithPortBinding(SqlPort, 1433)
                .WithEnvironment("MSSQL_PID", "Express");
            _container = containerBuilder.Build();
            await _container.StartAsync();
        }

        public void StartApi()
        {
            _apiProcess = Process.Start(new ProcessStartInfo(Path.GetFullPath("../../../../home-ca-backend.Api/bin/Debug/net8.0/home-ca-backend.Api.exe"))
            {
                WorkingDirectory = "../../../../home-ca-backend.Api/bin/Debug/net8.0/",
                Environment = { ["ASPNETCORE_URLS"] = $"http://*:{KestrelPort}/" }
            });
            _apiProcess.Should().NotBeNull();

            HttpClient = new()
            {
                BaseAddress = new($"http://localhost:{KestrelPort}")
            };
        }

        public async Task SendHttpRequest(string uri)
        {
            HttpClient.Should().NotBeNull();
            
            using HttpRequestMessage request = new(HttpMethod.Get, uri);
            var response = await HttpClient!.SendAsync(request);
            LastStatusCode = response.StatusCode;
        }

        public async Task DisposeAsync()
        {
            if (_container != null)
            {
                await _container.DisposeAsync();
            }
            
            _apiProcess?.Kill();
            _instance = null;
        }
    }
}