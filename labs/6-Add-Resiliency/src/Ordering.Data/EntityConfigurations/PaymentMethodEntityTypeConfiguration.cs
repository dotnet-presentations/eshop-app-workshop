using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Ordering.Data.EntityConfigurations;

class PaymentMethodEntityTypeConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> paymentConfiguration)
    {
        paymentConfiguration.ToTable("paymentmethods");

        paymentConfiguration.Property("Id")
            .UseHiLo("paymentseq");

        paymentConfiguration.Property("BuyerId")
            .HasColumnName("BuyerId");

        paymentConfiguration
            .Property("Alias")
            .HasColumnName("Alias")
            .HasMaxLength(200);

        paymentConfiguration
            .Property("CardHolderName")
            .HasMaxLength(200)
            .IsRequired();

        paymentConfiguration
            .Property("CardNumber")
            .HasColumnName("CardNumber")
            .HasMaxLength(25)
            .IsRequired();

        paymentConfiguration
            .Property("Expiration")
            .HasColumnName("Expiration")
            .HasMaxLength(25)
            .IsRequired();

        paymentConfiguration
            .Property("CardTypeId")
            .HasColumnName("CardTypeId");

        paymentConfiguration.HasOne<CardType>("CardType")
            .WithMany()
            .HasForeignKey("CardTypeId");
    }
}
