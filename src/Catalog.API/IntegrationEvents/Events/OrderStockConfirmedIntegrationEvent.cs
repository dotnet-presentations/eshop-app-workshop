using eShop.EventBus.Events;
namespace eShop.Catalog.API.IntegrationEvents.Events;

public record OrderStockConfirmedIntegrationEvent(int OrderId) : IntegrationEvent;
