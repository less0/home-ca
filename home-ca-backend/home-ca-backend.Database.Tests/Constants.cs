namespace home_ca_backend.Database.Tests;

public class Constants
{
    public static readonly int Port = 60000 + Random.Shared.Next(5535);
    public static readonly string ConnectionString = $"Server=localhost,{Port};User Id=sa;Password=2Pq93JS!;TrustServerCertificate=True";
    public static readonly string DatabaseConnectionString = $"{ConnectionString};Database=home-ca";
}