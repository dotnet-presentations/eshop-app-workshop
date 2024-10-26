namespace Catalog.API.Models
{
    public readonly struct PaginationRequest(int pageSize = 10, int pageIndex = 0)
    {
        public readonly int PageSize { get; } = pageSize;

        public readonly int PageIndex { get; } = pageIndex;
    }
}
