using Microsoft.EntityFrameworkCore;
using Zenatur.Tms.Infrastructure.Persistence.Entities;

namespace Zenatur.Tms.Infrastructure.Persistence;

public class TmsDbContext : DbContext
{
    public TmsDbContext(DbContextOptions<TmsDbContext> options) : base(options) { }

    public DbSet<TmsUser> Users => Set<TmsUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TmsUser>(e =>
        {
            e.ToTable("Users", "Auth");
            e.HasKey(u => u.Id);
            e.Property(u => u.ExternalId).HasMaxLength(100).IsRequired();
            e.HasIndex(u => u.ExternalId).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.Property(u => u.CreatedAt).HasColumnType("datetime2");
        });
    }
}
