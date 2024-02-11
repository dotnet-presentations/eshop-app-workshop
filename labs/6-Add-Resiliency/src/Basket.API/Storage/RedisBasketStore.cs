using System.Text.Json;
using eShop.Basket.API.Models;
using StackExchange.Redis;

namespace eShop.Basket.API.Storage;

public class RedisBasketStore(IConnectionMultiplexer redis) : IBasketStore
{
    private readonly IDatabase _database = redis.GetDatabase();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly RedisKey BasketKeyPrefix = "/basket/";

    private static RedisKey GetBasketKey(string userId) => BasketKeyPrefix.Append(userId);

    public async Task<CustomerBasket?> GetBasketAsync(string customerId)
    {
        var key = GetBasketKey(customerId);

        using var data = await _database.StringGetLeaseAsync(key);

        return data is { Length: > 0 }
            ? JsonSerializer.Deserialize<CustomerBasket>(data.Span, _jsonOptions)
            : null;
    }

    public async Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(basket, _jsonOptions);
        var key = GetBasketKey(basket.BuyerId);

        var created = await _database.StringSetAsync(key, json);

        return created
            ? await GetBasketAsync(basket.BuyerId)
            : null;
    }

    public async Task<bool> DeleteBasketAsync(string id)
    {
        return await _database.KeyDeleteAsync(GetBasketKey(id));
    }
}
