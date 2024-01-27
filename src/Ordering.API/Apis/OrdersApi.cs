using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eShop.Ordering.API.Apis;
using eShop.Ordering.API.Data;
using eShop.Ordering.API.Application.Models;

namespace Microsoft.AspNetCore.Builder;

public static class OrdersApi
{
    public static RouteGroupBuilder MapOrdersApi(this RouteGroupBuilder app)
    {
        app.MapPost("/", CreateOrderAsync);
        app.MapGet("/", GetOrdersByUserAsync);

        return app;
    }

    public static async Task<Ok<OrderSummary[]>> GetOrdersByUserAsync([AsParameters] OrderServices services)
    {
        var userId = services.IdentityService.GetUserIdentity();

        var orders = await services.DbContext.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Buyer)
            .Where(o => o.Buyer.IdentityGuid == userId)
            .Select(o => OrderSummary.FromOrder(o))
            .ToArrayAsync();

        return TypedResults.Ok(orders);
    }

    public static async Task<Results<Ok, BadRequest<string>>> CreateOrderAsync(
        [FromHeader(Name = "x-requestid")] Guid requestId,
        CreateOrderRequest request,
        [AsParameters] OrderServices services)
    {
        var userId = services.IdentityService.GetUserIdentity()
            ?? throw new InvalidOperationException("User identity could not be found. This endpoint requires authorization.");

        if (requestId == Guid.Empty)
        {
            services.Logger.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", request);
            return TypedResults.BadRequest("RequestId is missing.");
        }

        // TODO: Validation

        var requestPaymentMethod = new PaymentMethod
        {
            CardTypeId = request.CardTypeId,
            CardHolderName = request.CardHolderName,
            CardNumber = request.CardNumber,
            Expiration = request.CardExpiration,
            SecurityNumber = request.CardSecurityNumber,
        };

        var buyer = await services.DbContext.Buyers
            .Where(b => b.IdentityGuid == userId)
            .Include(b => b.PaymentMethods
                .Where(pm => pm.CardTypeId == requestPaymentMethod.CardTypeId
                             && pm.CardNumber == requestPaymentMethod.CardNumber
                             && pm.Expiration == requestPaymentMethod.Expiration))
            .SingleOrDefaultAsync();

        if (buyer is null)
        {
            buyer = new Buyer
            {
                IdentityGuid = userId,
                Name = request.UserName
            };
            services.DbContext.Buyers.Add(buyer);
        }

        if (buyer.PaymentMethods.SingleOrDefault() is null)
        {
            buyer.PaymentMethods.Add(new PaymentMethod
            {
                CardTypeId = request.CardTypeId,
                CardNumber = request.CardNumber,
                CardHolderName = request.CardHolderName,
                Expiration = request.CardExpiration,
                SecurityNumber = request.CardSecurityNumber
            });
        }

        var paymentMethod = buyer.PaymentMethods.Single();

        var order = new Order
        {
            Buyer = buyer,
            Address = new Address(request.Street, request.City, request.State, request.Country, request.ZipCode),
            OrderItems = request.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Units = i.Quantity,
                Discount = 0
            }).ToList(),
            PaymentMethod = paymentMethod
        };

        services.DbContext.Orders.Add(order);

        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok();
    }
}

public record CreateOrderRequest(
    string UserName,
    string City,
    string Street,
    string State,
    string Country,
    string ZipCode,
    string CardNumber,
    string CardHolderName,
    DateTime CardExpiration,
    string CardSecurityNumber,
    int CardTypeId,
    IReadOnlyCollection<BasketItem> Items);
