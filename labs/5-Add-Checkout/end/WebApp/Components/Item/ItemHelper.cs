using eShop.WebApp.Components.Catalog;

namespace eShop.WebAppComponents.Item;

public static class ItemHelper
{
    public static string Url(CatalogItem item)
        => $"item/{item.Id}";
}
