namespace ExpenseTracker.Application.Common;

public abstract class PagedQuery
{
    private const int DefaultPageSize = 20;
    private const int MaximumPageSize = 100;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    public int Page
    {
        get => _page;
        set => _page = value < 1
            ? 1
            : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => DefaultPageSize,
            > MaximumPageSize => MaximumPageSize,
            _ => value
        };
    }

    public string SortBy { get; set; } = "date";

    public string SortDirection { get; set; } = "desc";
}