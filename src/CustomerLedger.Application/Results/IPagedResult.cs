namespace CustomerLedger.Application.Results;

/// <summary>Non-generic view of PagedResult&lt;T&gt;'s paging metadata, so a single Razor partial (_Pagination) can render the pager for any list type without generic-variance issues.</summary>
public interface IPagedResult
{
    int TotalCount { get; }
    int PageNumber { get; }
    int PageSize { get; }
    int TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}
