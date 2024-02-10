namespace eShop.WebApp.Services;

public class BasketService
{
    public async Task<IReadOnlyCollection<BasketQuantity>> GetBasketAsync()
    {
        await Task.CompletedTask;
        return [];
    }

    public async Task UpdateBasketAsync(IReadOnlyCollection<BasketQuantity> basket)
    {
        await Task.CompletedTask;
    }
}

public record BasketQuantity(int ProductId, int Quantity);
