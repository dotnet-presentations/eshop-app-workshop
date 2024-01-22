public class CatalogServices(
    CatalogContext context,
    IOptions<CatalogOptions> options,
    ILogger<CatalogServices> logger
    )
{
    public CatalogContext Context { get; } = context;
    public IOptions<CatalogOptions> Options { get; } = options;
    public ILogger<CatalogServices> Logger { get; } = logger;
};
