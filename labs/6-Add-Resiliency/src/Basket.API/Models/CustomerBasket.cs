namespace eShop.Basket.API.Models;

public class CustomerBasket
{
    public CustomerBasket() { }

    public CustomerBasket(string customerId)
    {
        BuyerId = customerId;
    }

    public required string BuyerId { get; set; }

    public List<BasketItem> Items { get; set; } = [];
}
