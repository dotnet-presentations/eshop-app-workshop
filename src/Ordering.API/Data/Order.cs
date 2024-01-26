using System.ComponentModel.DataAnnotations;

namespace eShop.Ordering.API.Data;

public class Order
{
    public int Id { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required]
    public Address Address { get; set; }

    public int? BuyerId { get; set; }

    public virtual Buyer Buyer { get; set; }

    public OrderStatus OrderStatus { get; private set; } = OrderStatus.Submitted;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = [];

    public int? PaymentMethodId { get; set; }

    public virtual PaymentMethod PaymentMethod { get; set; }

    internal decimal GetTotal()
    {
        return OrderItems?.Sum(o => (o.Units * o.UnitPrice) - o.Discount) ?? 0;
    }
}
