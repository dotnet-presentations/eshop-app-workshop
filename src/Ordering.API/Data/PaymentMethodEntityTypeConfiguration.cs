using eShop.Ordering.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Ordering.API.Data;

class PaymentMethodEntityTypeConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> paymentConfiguration)
    {
        paymentConfiguration.ToTable("paymentmethods");

        paymentConfiguration.Property(b => b.Id)
            .UseHiLo("paymentseq");

        paymentConfiguration.Property<int>("BuyerId");

        paymentConfiguration.HasOne<Buyer>()
            .WithMany()
            .HasForeignKey("BuyerId");

        paymentConfiguration
            .Property("CardHolderName")
            .HasMaxLength(200);

        paymentConfiguration
            .Property("Alias")
            .HasColumnName("Alias")
            .HasMaxLength(200);

        paymentConfiguration
            .Property("CardNumber")
            .HasColumnName("CardNumber")
            .HasMaxLength(25)
            .IsRequired();

        paymentConfiguration
            .Property("Expiration")
            .HasColumnName("Expiration")
            .HasMaxLength(25);

        paymentConfiguration
            .Property("CardTypeId")
            .HasColumnName("CardTypeId");

        paymentConfiguration.HasOne(p => p.CardType)
            .WithMany()
            .HasForeignKey("CardTypeId");
    }
}
