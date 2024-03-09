using home_ca_backend.Core.CertificateAuthorityServerAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace home_ca_backend.Database;

public class CertificateAuthorityContext : DbContext
{
    private readonly IConfiguration _config;

    public CertificateAuthorityContext(IConfiguration config)
    {
        _config = config;
    }

    public CertificateAuthorityContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_config.GetConnectionString("(Default)"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CertificateAuthority>()
            .HasKey(ca => ca.Id);
        modelBuilder.Entity<CertificateAuthority>()
            .Property(ca => ca.Id)
            .HasConversion(
                certificateAuthorityId => certificateAuthorityId.Guid,
                guid => new(guid));
        modelBuilder.Entity<Leaf>()
            .HasKey(leaf => leaf.Id);
        modelBuilder.Entity<Leaf>()
            .Property(leaf => leaf.Id)
            .HasConversion(
                leafId => leafId.Guid,
                guid => new(guid));
    }
}