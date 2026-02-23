using Microsoft.EntityFrameworkCore;
using ObservabilityPOC.Api.Models;

namespace ObservabilityPOC.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Tickets");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Email).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Address).IsRequired().HasMaxLength(300);
            entity.Property(t => t.Occurred).IsRequired().HasMaxLength(1000);
            entity.Property(t => t.CreatedAt).IsRequired();
        });
    }
}
