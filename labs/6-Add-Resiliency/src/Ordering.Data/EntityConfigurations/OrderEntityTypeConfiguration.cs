using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Ordering.Data.EntityConfigurations;

class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> orderConfiguration)
    {
        orderConfiguration.ToTable("orders");

        orderConfiguration.Property("Id")
            .UseHiLo("orderseq");

        // Address value object persisted as owned entity
        var addressConfiguration = orderConfiguration
            .OwnsOne<Address>("Address");

        addressConfiguration.Property("Street").HasMaxLength(200);
        addressConfiguration.Property("City").HasMaxLength(100);
        addressConfiguration.Property("State").HasMaxLength(60);
        addressConfiguration.Property("Country").HasMaxLength(90);
        addressConfiguration.Property("ZipCode").HasMaxLength(18);

        orderConfiguration
            .Property("BuyerId")
            .HasColumnName("BuyerId");

        orderConfiguration.HasOne<Buyer>("Buyer")
            .WithMany()
            .HasForeignKey("BuyerId");

        orderConfiguration
            .Property("OrderDate")
            .HasColumnName("OrderDate");

        orderConfiguration
            .Property("OrderStatus")
            .HasConversion<string>()
            .HasMaxLength(30);

        orderConfiguration.HasMany<OrderItem>("OrderItems")
            .WithOne("Order");

        orderConfiguration
            .Property("PaymentMethodId")
            .HasColumnName("PaymentMethodId");

        orderConfiguration.HasOne<PaymentMethod>("PaymentMethod")
            .WithMany()
            .HasForeignKey("PaymentMethodId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
