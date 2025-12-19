
namespace TijarahJoDBAPI.DTOs.Requests;

/*
	Found the issue! The GetPaginatedRequest class uses fields instead of properties. 
		ASP.NET Core model binding only works with properties (with { get; set; }).

	The problem: Your class had fields (public int PageNumber;) instead of properties (public int PageNumber { get; set; }).
*/
public class GetPaginatedRequest
{
	public int PageNumber { get; set; } = 1;
	public int RowsPerPage { get; set; } = 10;
	public bool IncludeDeleted { get; set; } = false;
}