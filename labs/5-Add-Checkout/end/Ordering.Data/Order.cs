using System.ComponentModel.DataAnnotations;

namespace eShop.Ordering.Data;

public class Order
{
    public int Id { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required]
    public required Address Address { get; set; }

    public int? BuyerId { get; set; }

    public required Buyer Buyer { get; set; }

    public OrderStatus OrderStatus { get; private set; } = OrderStatus.Submitted;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = [];

    public int? PaymentMethodId { get; set; }

    public required PaymentMethod PaymentMethod { get; set; }

    public decimal GetTotal()
    {
        return OrderItems?.Sum(o => (o.Units * o.UnitPrice) - o.Discount) ?? 0;
    }
}
