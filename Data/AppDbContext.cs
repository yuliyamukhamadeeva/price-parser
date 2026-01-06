using Microsoft.EntityFrameworkCore;
using PriceParser.Web.Models;

namespace PriceParser.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductLink> ProductLinks => Set<ProductLink>();
    public DbSet<PriceLog> PriceLogs => Set<PriceLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shop>()
            .HasIndex(s => s.Code)
            .IsUnique();

        modelBuilder.Entity<ProductLink>()
            .HasIndex(l => new { l.ProductId, l.ShopId, l.Url })
            .IsUnique();
    }
}
