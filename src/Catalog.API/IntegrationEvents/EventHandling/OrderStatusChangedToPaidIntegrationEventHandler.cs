using eShop.Catalog.API.Infrastructure;
using eShop.Catalog.API.IntegrationEvents.Events;
using eShop.EventBus.Abstractions;

namespace eShop.Catalog.API.IntegrationEvents.EventHandling;

public class OrderStatusChangedToPaidIntegrationEventHandler(
    CatalogContext catalogContext,
    ILogger<OrderStatusChangedToPaidIntegrationEventHandler> logger) :
    IIntegrationEventHandler<OrderStatusChangedToPaidIntegrationEvent>
{
    public async Task Handle(OrderStatusChangedToPaidIntegrationEvent @event)
    {
        logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

        // We're not blocking stock/inventory
        foreach (var orderStockItem in @event.OrderStockItems)
        {
            var catalogItem = catalogContext.CatalogItems.Find(orderStockItem.ProductId);

            if (catalogItem is null)
            {
                logger.LogWarning("Catalog item with ProductId '{ProductId}' not found", orderStockItem.ProductId);
                continue;
            }

            catalogItem.RemoveStock(orderStockItem.Units);
        }

        await catalogContext.SaveChangesAsync();
    }
}
