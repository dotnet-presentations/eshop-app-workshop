using eShop.Catalog.API.Infrastructure;
using eShop.Catalog.API.IntegrationEvents.Events;
using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;

namespace eShop.Catalog.API.IntegrationEvents.EventHandling;

public class OrderStatusChangedToAwaitingValidationIntegrationEventHandler(
    CatalogContext catalogContext,
    ICatalogIntegrationEventService catalogIntegrationEventService,
    ILogger<OrderStatusChangedToAwaitingValidationIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderStatusChangedToAwaitingValidationIntegrationEvent>
{
    public async Task Handle(OrderStatusChangedToAwaitingValidationIntegrationEvent @event)
    {
        logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

        var confirmedOrderStockItems = new List<ConfirmedOrderStockItem>();

        foreach (var orderStockItem in @event.OrderStockItems)
        {
            var catalogItem = catalogContext.CatalogItems.Find(orderStockItem.ProductId);

            if (catalogItem is null)
            {
                logger.LogWarning("Catalog item with ProductId '{ProductId}' not found", orderStockItem.ProductId);
                continue;
            }

            var hasStock = catalogItem.AvailableStock >= orderStockItem.Units;
            var confirmedOrderStockItem = new ConfirmedOrderStockItem(catalogItem.Id, hasStock);

            confirmedOrderStockItems.Add(confirmedOrderStockItem);
        }

        var confirmedIntegrationEvent = confirmedOrderStockItems.Any(c => !c.HasStock)
            ? (IntegrationEvent)new OrderStockRejectedIntegrationEvent(@event.OrderId, confirmedOrderStockItems)
            : new OrderStockConfirmedIntegrationEvent(@event.OrderId);

        await catalogIntegrationEventService.SaveEventAndCatalogContextChangesAsync(confirmedIntegrationEvent);
        await catalogIntegrationEventService.PublishThroughEventBusAsync(confirmedIntegrationEvent);
    }
}
