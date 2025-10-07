namespace Lms.Api.Responses;

public sealed class PagedResult<T>
{
    public PagedResult(long totalCount, int pageNumber, int pageSize, IReadOnlyCollection<T> items)
    {
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        Items = items;
    }

    public long TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public IReadOnlyCollection<T> Items { get; }
}
