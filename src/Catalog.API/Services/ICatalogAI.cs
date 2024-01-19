//using Pgvector;

namespace eShop.Catalog.API.Services;

public interface ICatalogAI
{
    public bool IsEnabled { get { return false; } }
    //ValueTask<Vector> GetEmbeddingAsync(string text);
    //ValueTask<Vector> GetEmbeddingAsync(CatalogItem item);
}
