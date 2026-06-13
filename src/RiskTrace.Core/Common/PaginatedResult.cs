namespace RiskTrace.Core.Common;

public sealed class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; }

    //Offset Pagination
    public int TotalCount { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalPages =>
        (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;

    //Cursor Pagination
    public bool HasNextPage { get; }

    public string? NextCursor { get; }

    public PaginatedResult(
        IReadOnlyList<T> items,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        HasNextPage = PageNumber < TotalPages;
    }

    public PaginatedResult(
        IReadOnlyList<T> items,
        string? nextCursor,
        bool hasNextPage)
    {
        Items = items;
        TotalCount = 0;
        PageNumber = 1;
        PageSize = items.Count;
        NextCursor = nextCursor;
        HasNextPage = hasNextPage;
    }
}