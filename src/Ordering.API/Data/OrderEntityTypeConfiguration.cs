using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Ordering.API.Data;

class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> orderConfiguration)
    {
        orderConfiguration.ToTable("orders");

        orderConfiguration.Property(o => o.Id)
            .UseHiLo("orderseq");

        // Address value object persisted as owned entity type supported since EF Core 2.0
        orderConfiguration
            .OwnsOne(o => o.Address);

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
            .Property(o => o.OrderStatus)
            .HasConversion<string>()
            .HasMaxLength(30);

        orderConfiguration
            .Property("PaymentMethodId")
            .HasColumnName("PaymentMethodId");

        orderConfiguration.HasMany<OrderItem>("OrderItems")
            .WithOne("Order");

        orderConfiguration.HasOne<PaymentMethod>()
            .WithMany()
            .HasForeignKey("PaymentMethodId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
