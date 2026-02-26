using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BusinessAnalytics.API.Models;

namespace BusinessAnalytics.API.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<ImportSession> ImportSessions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Custom configurations
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId);

        // Store Enum as string in database
        builder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();
        
        // ImportSession -> Orders (one-to-many)
        builder.Entity<Order>()
            .HasOne(o => o.ImportSession)
            .WithMany(s => s.Orders)
            .HasForeignKey(o => o.ImportSessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<AuditLog>()
            .Property(a => a.Action)
            .HasConversion<string>();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Set global decimal precision (18,2)
        configurationBuilder.Properties<decimal>()
            .HavePrecision(18, 2);
    }
}
