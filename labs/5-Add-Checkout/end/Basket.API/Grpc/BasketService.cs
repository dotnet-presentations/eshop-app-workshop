using System.Diagnostics.CodeAnalysis;
using eShop.Basket.API.Models;
using eShop.Basket.API.Storage;
using Grpc.Core;

namespace eShop.Basket.API.Grpc;

public class BasketService(RedisBasketStore basketStore) : Basket.BasketBase
{
    public override async Task<CustomerBasketResponse> GetBasket(GetBasketRequest request, ServerCallContext context)
    {
        var userId = context.GetUserIdentity();

        if (string.IsNullOrEmpty(userId))
        {
            ThrowNotAuthenticated();
        }

        var data = await basketStore.GetBasketAsync(userId);

        return data is not null
            ? MapToCustomerBasketResponse(data)
            : new();
    }

    public override async Task<CustomerBasketResponse> UpdateBasket(UpdateBasketRequest request, ServerCallContext context)
    {
        var userId = context.GetUserIdentity();

        if (string.IsNullOrEmpty(userId))
        {
            ThrowNotAuthenticated();
        }

        var customerBasket = MapToCustomerBasket(userId, request);
        var response = await basketStore.UpdateBasketAsync(customerBasket);

        if (response is null)
        {
            ThrowBasketDoesNotExist(userId);
        }

        return MapToCustomerBasketResponse(response);
    }

    public override async Task<DeleteBasketResponse> DeleteBasket(DeleteBasketRequest request, ServerCallContext context)
    {
        var userId = context.GetUserIdentity();

        if (string.IsNullOrEmpty(userId))
        {
            ThrowNotAuthenticated();
        }

        await basketStore.DeleteBasketAsync(userId);

        return new();
    }

    [DoesNotReturn]
    private static void ThrowNotAuthenticated()
        => throw new RpcException(new Status(StatusCode.Unauthenticated, "The caller is not authenticated."));

    [DoesNotReturn]
    private static void ThrowBasketDoesNotExist(string userId)
        => throw new RpcException(new Status(StatusCode.NotFound, $"Basket with buyer ID {userId} does not exist"));

    private static CustomerBasketResponse MapToCustomerBasketResponse(CustomerBasket customerBasket)
    {
        var response = new CustomerBasketResponse();

        foreach (var item in customerBasket.Items)
        {
            response.Items.Add(new BasketItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
            });
        }

        return response;
    }

    private static CustomerBasket MapToCustomerBasket(string userId, UpdateBasketRequest customerBasketRequest)
    {
        var response = new CustomerBasket { BuyerId = userId };

        foreach (var item in customerBasketRequest.Items)
        {
            response.Items.Add(new()
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
            });
        }

        return response;
    }
}
