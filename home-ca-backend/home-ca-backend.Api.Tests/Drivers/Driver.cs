using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using home_ca_backend.Tests.Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reqnroll;
using Reqnroll.Assist;
using Testcontainers.MsSql;

namespace home_ca_backend.Api.Tests.Drivers
{
    public class Driver
    {
        private static Driver? _instance;
        private static IConfiguration _configuration = null!;
        
        public static Driver Instance => _instance ??= new();
        
        private static readonly int SqlPort = 50000 + Random.Shared.Next(9999);
        private static readonly int KestrelPort = 40000 + Random.Shared.Next(9999);
        
        private MsSqlContainer? _container;
        private Process? _apiProcess;
        private string? _accessToken;

        public HttpClient? HttpClient { get; private set; }

        public HttpStatusCode LastStatusCode { get; set; }
        public string LastResponseBody { get; set; }

        public string ConnectionString { get; } =
            $"Server=localhost,{SqlPort};User Id=sa;Password=2Pq93JS!;TrustServerCertificate=True;Database=home-ca";

        public RawDatabaseAccess RawDatabaseAccess { get; set; }
        public bool IsAuthenticated => _accessToken != null;

        private Driver()
        {
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.AddJsonFile("settings.json", false);
            configurationBuilder.AddJsonFile("settings.local.json", true);
            configurationBuilder.AddEnvironmentVariables();
            _configuration = configurationBuilder.Build();
            RawDatabaseAccess = new RawDatabaseAccess(ConnectionString);
        }

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
            AutoResetEvent apiReadyWaitHandle = new AutoResetEvent(false);

            _apiProcess = Process.Start(new ProcessStartInfo(Path.GetFullPath("../../../../home-ca-backend.Api/bin/Debug/net8.0/home-ca-backend.Api.exe"))
            {
                WorkingDirectory = "../../../../home-ca-backend.Api/bin/Debug/net8.0/",
                Environment =
                {
                    ["ASPNETCORE_URLS"] = $"http://*:{KestrelPort}/",
                    ["ASPNETCORE_ENVIRONMENT"] = "Development",
                    ["ConnectionStrings__(DEFAULT)"] = $"Server=localhost,{SqlPort};Database=home-ca;User Id=sa;Password=2Pq93JS!;TrustServerCertificate=True"
                },
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            _apiProcess.Should().NotBeNull();
            _apiProcess!.OutputDataReceived += (_, args) =>
            {
                if (args.Data == null)
                {
                    return;
                }
                
                if (args.Data.Contains("Application started."))
                {
                    apiReadyWaitHandle.Set();
                }
            };
            _apiProcess.BeginOutputReadLine();

            HttpClient = new()
            {
                BaseAddress = new($"http://localhost:{KestrelPort}")
            };

            if (!apiReadyWaitHandle.WaitOne(TimeSpan.FromSeconds(30)))
            {
                throw new TimeoutException();
            }
            _apiProcess.CancelOutputRead();
        }

        public async Task Authenticate(string username, string password)
        {
            // Due to security considerations the values required for authentication are not included here,
            // but are loaded from configuration. For local development, they can be loaded from settings.local.json
            // otherwise it's also possible to use environment variables.
            
            HttpClient.Should().NotBeNull();
            var auth0Section = _configuration.GetRequiredSection("Auth0");
            var authority = auth0Section["authority"];
            var clientId = auth0Section["client_id"];
            var clientSecret = auth0Section["client_secret"];
            var audience = auth0Section["audience"];

            authority.Should().NotBeNull();
            clientId.Should().NotBeNull();
            clientSecret.Should().NotBeNull();
            audience.Should().NotBeNull();
            
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                $"{authority}oauth/token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                ["grant_type"] = "password",
                ["client_id"] = clientId!,
                ["client_secret"] = clientSecret!,
                ["audience"] = audience!,
                ["username"] = username,
                ["password"] = password
            });

            var response = await HttpClient!.SendAsync(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var jObject = JsonConvert.DeserializeObject<JObject>(content);
            _accessToken = jObject?.GetValue("access_token")?.Value<string>();
            _accessToken.Should().NotBeNull();
        }

        public async Task SendHttpRequest(string uri)
        {
            HttpClient.Should().NotBeNull();
            
            using HttpRequestMessage request = new(HttpMethod.Get, uri);
            if (_accessToken != null)
            {
                request.Headers.Authorization = new("Bearer", _accessToken);
            }
            var response = await HttpClient!.SendAsync(request);
            LastStatusCode = response.StatusCode;
            LastResponseBody = await response.Content.ReadAsStringAsync();
        }

        public async Task SendHttpPostRequest(string uri, HttpContent content)
        {
            HttpClient.Should().NotBeNull();
            
            using HttpRequestMessage request = new(HttpMethod.Post, uri);
            request.Content = content;
            if (_accessToken != null)
            {
                request.Headers.Authorization = new("Bearer", _accessToken);
            }
            var response = await HttpClient!.SendAsync(request);
            LastStatusCode = response.StatusCode;
            LastResponseBody = await response.Content.ReadAsStringAsync();
        }

        public async Task DisposeAsync()
        {
            if(_apiProcess != null)
            {
                _apiProcess.Kill();
                await _apiProcess.WaitForExitAsync();
            }
            
            if (_container != null)
            {
                await _container.DisposeAsync();
            }
            
            _instance = null;
        }

        public void AssertHasWellFormedCertificateForLastReturnedId(string expectedName)
        {
            CertificateAuthorityId id = new(new(LastResponseBody));

            var name = RawDatabaseAccess.GetReferenceValueByTableAndId<CertificateAuthority, string>(id,
                nameof(CertificateAuthority.Name));
            var createdAt =
                RawDatabaseAccess.GetValueByTableAndId<CertificateAuthority, DateTimeOffset>(id,
                    nameof(CertificateAuthority.CreatedAt));
            var privateKey =
                RawDatabaseAccess.GetReferenceValueByTableAndId<CertificateAuthority, string>(id,
                    nameof(CertificateAuthority.PemPrivateKey));
            var certificate =
                RawDatabaseAccess.GetReferenceValueByTableAndId<CertificateAuthority, string>(id,
                    nameof(CertificateAuthority.PemCertificate));

            name.Should().Be(expectedName);
            createdAt.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(10));
            privateKey.Should().StartWith("-----BEGIN ENCRYPTED PRIVATE KEY-----\n");
            certificate.Should().StartWith("-----BEGIN CERTIFICATE-----\n");
        }

        public void AssertHasCertificateWithLifetimeForLastReturnedId(DateTime certificateLifetime)
        {
            CertificateAuthorityId id = new(new(LastResponseBody));
            var pemCertificate = RawDatabaseAccess.GetReferenceValueByTableAndId<CertificateAuthority, string>(id, 
                nameof(CertificateAuthority.PemCertificate));
            var certificate = X509Certificate2.CreateFromPem(pemCertificate);
            certificate.NotAfter.Should().BeCloseTo(certificateLifetime, TimeSpan.FromSeconds(10));
        }

        public void ResetHttpClient()
        {
            LastStatusCode = default;
            LastResponseBody = string.Empty;
        }

        public void RemoveAuthenticationToken()
        {
            _accessToken = null;
        }

        public void CreateLeafs(Table table)
        {
            foreach (var tableRow in table.Rows)
            {
                var id = tableRow.GetString("Id");
                var name = tableRow.GetString("Name");
                var parent = tableRow.GetString("Parent");
                
                RawDatabaseAccess.CreateLeaf(Guid.Parse(parent), Guid.Parse(id), name);
            }
        }
    }
}