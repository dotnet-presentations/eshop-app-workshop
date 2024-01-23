using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using eShop.Catalog.API.Model;

namespace eShop.Catalog.API.Infrastructure.EntityConfigurations;

class CatalogTypeEntityTypeConfiguration: IEntityTypeConfiguration<CatalogType>
{
    public void Configure(EntityTypeBuilder<CatalogType> builder)
    {
        builder.ToTable("CatalogType");

        builder.Property(cb => cb.Type)
            .HasMaxLength(100);
    }
}
