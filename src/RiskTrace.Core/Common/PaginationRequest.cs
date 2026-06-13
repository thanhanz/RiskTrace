namespace RiskTrace.Core.Common;

public sealed class PaginationRequest
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;
    private int _limit = 20;

    public int PageNumber { get; init; } = 1;

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public string? Cursor { get; init; }

    public int Limit
    {
        get => _limit;
        init => _limit = value > MaxPageSize ? MaxPageSize : value;
    }

    public string? Search { get; init; }

    public string? SortBy { get; init; }

    public bool IsDescending { get; init; }
}