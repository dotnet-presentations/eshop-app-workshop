namespace eShop.Catalog.API.Model;

public readonly struct PaginationRequest(int pageSize = 0, int pageIndex = 0)
{
    public readonly int PageSize { get; } = pageSize;

    public readonly int PageIndex { get; } = pageIndex;
}
