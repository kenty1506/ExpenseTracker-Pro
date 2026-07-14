namespace ExpenseTracker.Application.Common;

public class PagedResult<T>
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }

    public bool HasPreviousPage =>
        Page > 1;

    public bool HasNextPage =>
        Page < TotalPages;

    public int FirstItem =>
        TotalRecords == 0
            ? 0
            : ((Page - 1) * PageSize) + 1;

    public int LastItem =>
        TotalRecords == 0
            ? 0
            : Math.Min(
                Page * PageSize,
                TotalRecords);

    public List<T> Items { get; set; } = [];
}