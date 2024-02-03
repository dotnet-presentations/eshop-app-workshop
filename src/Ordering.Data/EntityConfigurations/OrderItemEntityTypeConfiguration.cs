using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Ordering.Data.EntityConfigurations;

class OrderItemEntityTypeConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> orderItemConfiguration)
    {
        orderItemConfiguration.ToTable("orderItems");

        orderItemConfiguration.Property(o => o.Id)
            .UseHiLo("orderitemseq");

        orderItemConfiguration
            .Property<int>("OrderId")
            .HasColumnName("OrderId");

        orderItemConfiguration.HasOne<Order>("Order")
            .WithMany("OrderItems")
            .HasForeignKey("OrderId");

        orderItemConfiguration
            .Property("Discount")
            .HasColumnName("Discount");

        orderItemConfiguration
            .Property("ProductName")
            .HasColumnName("ProductName");

        orderItemConfiguration
            .Property("UnitPrice")
            .HasColumnName("UnitPrice");

        orderItemConfiguration
            .Property("Units")
            .HasColumnName("Units");

        orderItemConfiguration
            .Property("PictureUrl")
            .HasColumnName("PictureUrl");
    }
}
