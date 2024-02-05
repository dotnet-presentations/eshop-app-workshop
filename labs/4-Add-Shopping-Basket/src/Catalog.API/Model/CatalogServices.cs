using Microsoft.Extensions.Options;
using eShop.Catalog.Data;

namespace eShop.Catalog.API.Model;

public readonly struct CatalogServices(CatalogDbContext dbContext, IOptions<CatalogOptions> options, ILogger<CatalogServices> logger)
{
    public CatalogDbContext DbContext { get; } = dbContext;

    public IOptions<CatalogOptions> Options { get; } = options;

    public ILogger<CatalogServices> Logger { get; } = logger;
};
