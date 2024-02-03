using System.ComponentModel.DataAnnotations;

namespace eShop.Ordering.Data;

public class OrderItem
{
    public int Id { get; set; }

    [Required]
    public required string ProductName { get; set; }

    public string? PictureUrl { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Discount { get; set; }

    public int Units { get; set; } = 1;

    public int ProductId { get; set; }

    public int OrderId { get; set; }

    public Order? Order { get; set; }
}
