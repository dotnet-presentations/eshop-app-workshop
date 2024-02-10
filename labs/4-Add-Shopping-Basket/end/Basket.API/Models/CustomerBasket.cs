namespace eShop.Basket.API.Models;

public class CustomerBasket(string customerId)
{
    public string BuyerId { get; } = customerId;

    public List<BasketItem> Items { get; } = [];
}
