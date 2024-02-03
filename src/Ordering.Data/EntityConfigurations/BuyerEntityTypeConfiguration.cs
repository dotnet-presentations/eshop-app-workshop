using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Ordering.Data.EntityConfigurations;

class BuyerEntityTypeConfiguration : IEntityTypeConfiguration<Buyer>
{
    public void Configure(EntityTypeBuilder<Buyer> buyerConfiguration)
    {
        buyerConfiguration.ToTable("buyers");

        buyerConfiguration.Property("Id")
            .UseHiLo("buyerseq");

        buyerConfiguration.Property("IdentityGuid")
            .HasMaxLength(200);

        buyerConfiguration.HasIndex("IdentityGuid")
            .IsUnique(true);

        buyerConfiguration.HasMany<PaymentMethod>("PaymentMethods")
            .WithOne()
            .HasForeignKey("BuyerId");
    }
}
