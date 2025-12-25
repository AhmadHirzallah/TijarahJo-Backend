using System.ComponentModel.DataAnnotations;

namespace TijarahJoDBAPI.DTOs.Requests;

/// <summary>
/// Request parameters for paginated post retrieval
/// </summary>
public class GetPaginatedRequest
{
    /// <summary>
    /// Page number (1-based). Defaults to 1.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be at least 1.")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page. Defaults to 10, maximum 200.
    /// </summary>
    [Range(1, 200, ErrorMessage = "RowsPerPage must be between 1 and 200.")]
    public int RowsPerPage { get; set; } = 10;

    /// <summary>
    /// Whether to include soft-deleted posts. Defaults to false.
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    /// <summary>
    /// Optional category filter. Null returns all categories.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "CategoryID must be a positive integer.")]
    public int? CategoryID { get; set; }
}