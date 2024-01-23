using eShop.Basket.API.IntegrationEvents.Events;
using eShop.Basket.API.Repositories;
using eShop.EventBus.Abstractions;

namespace eShop.Basket.API.IntegrationEvents.EventHandling;

public class OrderStartedIntegrationEventHandler(IBasketRepository repository, ILogger<OrderStartedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderStartedIntegrationEvent>
{
    public async Task Handle(OrderStartedIntegrationEvent @event)
    {
        logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

        await repository.DeleteBasketAsync(@event.UserId);
    }
}
