namespace eShop.Ordering.API.Application.Commands;

using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

// Regular CommandHandler
public class CreateOrderCommandHandler(
    IOrderingIntegrationEventService orderingIntegrationEventService,
    IOrderRepository orderRepository,
    ILogger<CreateOrderCommandHandler> logger) : IRequestHandler<CreateOrderCommand, bool>
{
    public async Task<bool> Handle(CreateOrderCommand message, CancellationToken cancellationToken)
    {
        // Add Integration event to clean the basket
        var orderStartedIntegrationEvent = new OrderStartedIntegrationEvent(message.UserId);
        await orderingIntegrationEventService.AddAndSaveEventAsync(orderStartedIntegrationEvent);

        // Add/Update the Buyer AggregateRoot
        // DDD patterns comment: Add child entities and value-objects through the Order Aggregate-Root
        // methods and constructor so validations, invariants and business logic 
        // make sure that consistency is preserved across the whole aggregate
        var address = new Address(message.Street, message.City, message.State, message.Country, message.ZipCode);
        var order = new Order(message.UserId, message.UserName, address, message.CardTypeId, message.CardNumber, message.CardSecurityNumber, message.CardHolderName, message.CardExpiration);

        foreach (var item in message.OrderItems)
        {
            order.AddOrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        logger.LogInformation("Creating Order - Order: {@Order}", order);

        orderRepository.Add(order);

        return await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}


// Use for Idempotency in Command process
public class CreateOrderIdentifiedCommandHandler(
    IMediator mediator,
    IRequestManager requestManager,
    ILogger<IdentifiedCommandHandler<CreateOrderCommand, bool>> logger)
    : IdentifiedCommandHandler<CreateOrderCommand, bool>(mediator, requestManager, logger)
{
    // Ignore duplicate requests for creating order.
    protected override bool CreateResultForDuplicateRequest() => true;
}
