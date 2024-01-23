using eShop.EventBus.Events;

namespace eShop.Catalog.API.IntegrationEvents.Events;

public record OrderStatusChangedToPaidIntegrationEvent(int OrderId, IEnumerable<OrderStockItem> OrderStockItems) : IntegrationEvent;
