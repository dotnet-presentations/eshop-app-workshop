using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eShop.Ordering.Data;
using eShop.Ordering.API.Model;
using MinimalApis.Extensions.Filters;

namespace Microsoft.AspNetCore.Builder;

public static class OrdersApi
{
    public static RouteGroupBuilder MapOrdersApi(this RouteGroupBuilder app)
    {
        app.MapGet("/", GetOrdersByUserAsync);

        app.MapPost("/", CreateOrderAsync)
            .WithParameterValidation(requireParameterAttribute: true);

        return app;
    }

    public static async Task<Results<Ok, BadRequest<string>, ValidationProblem>> CreateOrderAsync(
        [FromHeader(Name = "x-requestid")] Guid requestId,
        [Validate] CreateOrderRequest request,
        [AsParameters] OrderServices services)
    {
        var userId = services.IdentityService.GetUserIdentity()
            ?? throw new InvalidOperationException("User identity could not be found. This endpoint requires authorization.");

        if (requestId == Guid.Empty)
        {
            return TypedResults.BadRequest("RequestId is missing.");
        }

        if (!Enumeration.IsValid<CardType>(request.CardTypeId))
        {
            var errors = new Dictionary<string, string[]>
            {
                { nameof(CreateOrderRequest.CardTypeId), [$"Card type ID '{request.CardTypeId}' is invalid."] }
            };
            return TypedResults.ValidationProblem(errors);
        }

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
            // Include the payment method to check if it already exists
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

        var paymentMethod = buyer.PaymentMethods.SingleOrDefault();

        if (paymentMethod is null)
        {
            paymentMethod = new PaymentMethod
            {
                CardTypeId = request.CardTypeId,
                CardNumber = request.CardNumber,
                CardHolderName = request.CardHolderName,
                Expiration = request.CardExpiration,
                SecurityNumber = request.CardSecurityNumber
            };
            buyer.PaymentMethods.Add(paymentMethod);
        }

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

    public static async Task<Ok<OrderSummary[]>> GetOrdersByUserAsync([AsParameters] OrderServices services)
    {
        var userId = services.IdentityService.GetUserIdentity()
            ?? throw new InvalidOperationException("User identity could not be found. This endpoint requires authorization.");

        var orders = await services.DbContext.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Buyer)
            .Where(o => o.Buyer.IdentityGuid == userId)
            .Select(o => OrderSummary.FromOrder(o))
            .AsNoTracking()
            .ToArrayAsync();

        return TypedResults.Ok(orders);
    }
}
