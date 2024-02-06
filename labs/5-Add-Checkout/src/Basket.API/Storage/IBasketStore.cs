using eShop.Basket.API.Model;

namespace eShop.Basket.API.Storage;

public interface IBasketStore
{
    Task<CustomerBasket?> GetBasketAsync(string customerId);

    Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket);

    Task<bool> DeleteBasketAsync(string id);
}
