namespace eShop.Catalog.API.Model;

public class PaginatedItems<TItem>(int pageIndex, int pageSize, long count, IEnumerable<TItem> data) where TItem : class
{
    public int PageIndex { get; } = pageIndex;

    public int PageSize { get; } = pageSize;

    public long Count { get; } = count;

    public IEnumerable<TItem> Data { get;} = data;
}
