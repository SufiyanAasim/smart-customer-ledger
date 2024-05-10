namespace CustomerLedger.Application.Results;

/// <summary>
/// Standard shape for every paginated list query in the application, so list controllers
/// and their views share one pattern for search/sort/page instead of each screen inventing
/// its own paging contract.
/// </summary>
public class PagedResult<T> : IPagedResult
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
