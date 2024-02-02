using Microsoft.EntityFrameworkCore;
using eShop.Catalog.Data.EntityConfigurations;

namespace eShop.Catalog.Data;

/// <remarks>
/// Add migrations using the following command inside the 'Catalog.Data.Manager' project directory:
///
/// dotnet ef migrations add --context CatalogDbContext [migration-name]
/// </remarks>
public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<CatalogItem> CatalogItems { get; set; }

    public DbSet<CatalogBrand> CatalogBrands { get; set; }

    public DbSet<CatalogType> CatalogTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");

        modelBuilder.ApplyConfiguration(new CatalogBrandEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CatalogTypeEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CatalogItemEntityTypeConfiguration());
    }
}
