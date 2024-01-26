namespace eShop.Ordering.API.Application.Models;

public class BasketItem
{
    public required string Id { get; init; }
    public int ProductId { get; init; }
    public required string ProductName { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal OldUnitPrice { get; init; }
    public int Quantity { get; init; }
    public string? PictureUrl { get; init; }
}

