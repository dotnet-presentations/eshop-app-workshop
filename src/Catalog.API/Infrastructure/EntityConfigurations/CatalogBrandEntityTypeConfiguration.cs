using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using eShop.Catalog.API.Model;

namespace eShop.Catalog.API.Infrastructure.EntityConfigurations;

class CatalogBrandEntityTypeConfiguration : IEntityTypeConfiguration<CatalogBrand>
{
    public void Configure(EntityTypeBuilder<CatalogBrand> builder)
    {
        builder.ToTable("CatalogBrand");

        builder.Property(cb => cb.Brand)
            .HasMaxLength(100);
    }
}
