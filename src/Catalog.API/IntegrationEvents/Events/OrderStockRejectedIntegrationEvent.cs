using eShop.EventBus.Events;

namespace eShop.Catalog.API.IntegrationEvents.Events;

public record OrderStockRejectedIntegrationEvent(int OrderId, List<ConfirmedOrderStockItem> OrderStockItems) : IntegrationEvent;
