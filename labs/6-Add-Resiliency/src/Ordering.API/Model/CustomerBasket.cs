namespace eShop.Ordering.API.Model;

public class CustomerBasket(string buyerId, List<BasketItem> items)
{
    public string BuyerId { get; set; } = buyerId;

    public List<BasketItem> Items { get; set; } = items;
}
