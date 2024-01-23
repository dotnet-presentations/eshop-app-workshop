using eShop.EventBus.Events;

namespace eShop.Catalog.API.IntegrationEvents;

public interface ICatalogIntegrationEventService
{
    Task SaveEventAndCatalogContextChangesAsync(IntegrationEvent evt);

    Task PublishThroughEventBusAsync(IntegrationEvent evt);
}
