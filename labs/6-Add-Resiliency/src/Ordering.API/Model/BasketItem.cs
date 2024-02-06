using System.ComponentModel.DataAnnotations;

namespace eShop.Ordering.API.Model;

public class BasketItem
{
    [Required]
    public int ProductId { get; init; }

    [Required]
    public required string ProductName { get; init; }

    [Required, Range(0, double.MaxValue)]
    public decimal UnitPrice { get; init; }

    [Required]
    [Range(0, 10000)]
    public int Quantity { get; init; }
}

