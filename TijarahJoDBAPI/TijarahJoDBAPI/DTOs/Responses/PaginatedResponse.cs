namespace TijarahJoDBAPI.DTOs.Responses;

/// <summary>
/// Paginated response wrapper for list endpoints
/// </summary>
/// <typeparam name="T">Type of items in the list</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// The items for the current page
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int RowsPerPage { get; set; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => RowsPerPage > 0
        ? (int)Math.Ceiling((double)TotalCount / RowsPerPage)
        : 0;

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Creates a paginated response from items and pagination info
    /// </summary>
    public static PaginatedResponse<T> Create(
        List<T> items,
        int pageNumber,
        int rowsPerPage,
        int totalCount)
    {
        return new PaginatedResponse<T>
        {
            Items = items,
            PageNumber = pageNumber,
            RowsPerPage = rowsPerPage,
            TotalCount = totalCount
        };
    }
}
