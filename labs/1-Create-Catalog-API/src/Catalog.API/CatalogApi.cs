using Catalog.API.Models;
using eShop.Catalog.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API
{
    public static class CatalogApi
    {
        public static IEndpointRouteBuilder MapCatalogApi(this IEndpointRouteBuilder app)
        {
            app.MapGet("/items", async ([AsParameters] PaginationRequest paginationRequest, CatalogDbContext db) =>
            {
                var pageSize = paginationRequest.PageSize;
                var pageIndex = paginationRequest.PageIndex;

                var totalItems = await db.CatalogItems
                    .LongCountAsync();

                var itemsOnPage = await db.CatalogItems
                    .OrderBy(c => c.Name)
                    .Skip(pageSize * pageIndex)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                return new PaginatedItems<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);
            });
            return app;
        }
    }
}
