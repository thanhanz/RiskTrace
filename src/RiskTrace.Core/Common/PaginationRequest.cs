namespace RiskTrace.Core.Common;

public sealed class PaginationRequest
{
    private const int MaxPageSize = 100;

    public int PageNumber { get; init; } = 1;

    private int _pageSize = 10;

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public string? Search { get; init; }

    public string? SortBy { get; init; }

    public bool IsDescending { get; init; }
}