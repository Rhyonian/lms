namespace Lms.Api.Options;

public sealed class PaginationOptions
{
    public int DefaultPageSize { get; init; } = 20;
    public int MaxPageSize { get; init; } = 100;

    public (int PageNumber, int PageSize) Normalize(int pageNumber, int pageSize)
    {
        var normalizedPageNumber = pageNumber < 1 ? 1 : pageNumber;
        var normalizedPageSize = pageSize <= 0 ? DefaultPageSize : pageSize;
        if (normalizedPageSize > MaxPageSize)
        {
            normalizedPageSize = MaxPageSize;
        }

        return (normalizedPageNumber, normalizedPageSize);
    }
}
