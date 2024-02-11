namespace eShop.Basket.API.Models;

public class CustomerBasket
{
    public required string BuyerId { get; set; }

    public List<BasketItem> Items { get; set; } = [];
}
