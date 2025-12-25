using System.Data;
using TijarahJoDBAPI.DTOs.Responses;

namespace TijarahJoDBAPI.DTOs.Mappers;

/// <summary>
/// Mapper for converting DataRow to PostListingResponse
/// Used for paginated posts with enriched data from VW_PostsForListing
/// </summary>
public static class PostListingMapper
{
    /// <summary>
    /// Maps a DataRow from the paginated posts result to PostListingResponse
    /// </summary>
    public static PostListingResponse ToListingResponse(this DataRow row)
    {
        return new PostListingResponse
        {
            // Post Info
            PostID = row["PostID"] as int?,
            PostTitle = row["PostTitle"] as string ?? string.Empty,
            PostDescription = row["PostDescription"] == DBNull.Value ? null : row["PostDescription"] as string,
            Price = row["Price"] == DBNull.Value ? null : row["Price"] as decimal?,
            Status = row["Status"] is int status ? status : 0,
            CreatedAt = row["CreatedAt"] is DateTime createdAt ? createdAt : DateTime.MinValue,
            IsDeleted = row["IsDeleted"] is bool isDeleted && isDeleted,

            // User Info
            UserID = row["UserID"] is int userId ? userId : 0,
            Username = row["Username"] as string,
            Email = row["Email"] as string,
            FirstName = row["FirstName"] as string,
            LastName = row["LastName"] == DBNull.Value ? null : row["LastName"] as string,

            // Role Info
            RoleID = row["RoleID"] as int?,
            RoleName = row["RoleName"] as string,

            // Category Info
            CategoryID = row["CategoryID"] is int categoryId ? categoryId : 0,
            CategoryName = row["CategoryName"] as string
        };
    }

    /// <summary>
    /// Maps a DataTable to a list of PostListingResponse
    /// </summary>
    public static List<PostListingResponse> ToListingResponseList(this DataTable table)
    {
        var list = new List<PostListingResponse>();

        foreach (DataRow row in table.Rows)
        {
            list.Add(row.ToListingResponse());
        }

        return list;
    }

    /// <summary>
    /// Extracts the TotalRows value from the first row of the result
    /// </summary>
    public static int GetTotalRows(this DataTable table)
    {
        if (table.Rows.Count == 0)
            return 0;

        var firstRow = table.Rows[0];
        
        if (table.Columns.Contains("TotalRows") && firstRow["TotalRows"] is int totalRows)
            return totalRows;

        return table.Rows.Count;
    }
}
