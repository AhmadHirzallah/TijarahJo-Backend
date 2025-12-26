using System.Data;
using TijarahJoDB.DAL;
using TijarahJoDBAPI.DTOs.Responses;

namespace TijarahJoDBAPI.DTOs.Mappers;

/// <summary>
/// Mapper for PostDetails (from SP_GetPostDetails_All)
/// </summary>
public static class PostDetailsMapper
{
    /// <summary>
    /// Maps PostDetailsDataResult to PostDetailsResponse
    /// </summary>
    public static PostDetailsResponse? ToDetailsResponse(this PostDetailsDataResult data)
    {
        if (!data.HasData)
            return null;

        var postRow = data.PostDetails.Rows[0];

        var response = new PostDetailsResponse
        {
            // Post Info
            PostID = postRow["PostID"] is int postId ? postId : 0,
            PostTitle = postRow["PostTitle"] as string ?? string.Empty,
            PostDescription = postRow["PostDescription"] == DBNull.Value 
                ? null 
                : postRow["PostDescription"] as string,
            Price = postRow["Price"] == DBNull.Value 
                ? null 
                : postRow["Price"] as decimal?,
            Status = postRow["Status"] is int status ? status : 0,
            CreatedAt = postRow["CreatedAt"] is DateTime createdAt ? createdAt : DateTime.MinValue,
            IsDeleted = postRow["IsDeleted"] is bool isDeleted && isDeleted,

            // Owner Info
            OwnerUserID = postRow["OwnerUserID"] is int ownerId ? ownerId : 0,
            OwnerUsername = postRow["OwnerUsername"] as string,
            OwnerEmail = postRow["OwnerEmail"] as string,
            OwnerFirstName = postRow["OwnerFirstName"] as string,
            OwnerLastName = postRow["OwnerLastName"] == DBNull.Value 
                ? null 
                : postRow["OwnerLastName"] as string,
            OwnerFullName = postRow["OwnerFullName"] as string,

            // Role Info
            RoleID = postRow["RoleID"] as int?,
            RoleName = postRow["RoleName"] as string,

            // Category Info
            CategoryID = postRow["CategoryID"] is int categoryId ? categoryId : 0,
            CategoryName = postRow["CategoryName"] as string,

            // Reviews
            Reviews = MapReviews(data.Reviews),

            // Images
            Images = MapImages(data.Images)
        };

        return response;
    }

    private static List<ReviewDetailResponse> MapReviews(DataTable reviewsTable)
    {
        var reviews = new List<ReviewDetailResponse>();

        // Return empty list if table is null or has no columns (empty DataTable)
        if (reviewsTable == null || reviewsTable.Columns.Count == 0)
            return reviews;

        // Return empty list if no rows
        if (reviewsTable.Rows.Count == 0)
            return reviews;

        // Fail fast with clear message if wrong table was passed
        if (!reviewsTable.Columns.Contains("ReviewID") && !reviewsTable.Columns.Contains("UserID"))
        {
            var cols = string.Join(", ", reviewsTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            throw new ArgumentException(
                $"MapReviews received wrong DataTable. Expected Reviews result set with ReviewID/UserID columns. " +
                $"Received columns: [{cols}]. This usually means the stored procedure result sets are being read incorrectly.");
        }

        // Check which column schema we have (enriched vs basic)
        bool hasEnrichedColumns = reviewsTable.Columns.Contains("ReviewerUserID");

        foreach (DataRow row in reviewsTable.Rows)
        {
            if (hasEnrichedColumns)
            {
                // Enriched schema from SP_GetPostDetails_All with JOINs
                reviews.Add(new ReviewDetailResponse
                {
                    ReviewID = row["ReviewID"] is int reviewId ? reviewId : 0,
                    PostID = row["PostID"] is int postId ? postId : 0,
                    Rating = row["Rating"] is byte rating ? rating : (byte)0,
                    ReviewText = row["ReviewText"] == DBNull.Value ? null : row["ReviewText"] as string,
                    CreatedAt = row["CreatedAt"] is DateTime createdAt ? createdAt : DateTime.MinValue,
                    ReviewerUserID = row["ReviewerUserID"] is int userId ? userId : 0,
                    ReviewerUsername = row["ReviewerUsername"] as string,
                    ReviewerEmail = row["ReviewerEmail"] as string,
                    ReviewerFirstName = row["ReviewerFirstName"] as string,
                    ReviewerLastName = row["ReviewerLastName"] == DBNull.Value 
                        ? null 
                        : row["ReviewerLastName"] as string,
                    ReviewerFullName = row["ReviewerFullName"] as string
                });
            }
            else
            {
                // Basic schema - direct from TbPostReviews table
                reviews.Add(new ReviewDetailResponse
                {
                    ReviewID = row["ReviewID"] is int reviewId ? reviewId : 0,
                    PostID = row["PostID"] is int postId ? postId : 0,
                    Rating = row["Rating"] is byte rating ? rating : (byte)0,
                    ReviewText = row["ReviewText"] == DBNull.Value ? null : row["ReviewText"] as string,
                    CreatedAt = row["CreatedAt"] is DateTime createdAt ? createdAt : DateTime.MinValue,
                    ReviewerUserID = row["UserID"] is int userId ? userId : 0,
                    // These will be null when using basic schema
                    ReviewerUsername = null,
                    ReviewerEmail = null,
                    ReviewerFirstName = null,
                    ReviewerLastName = null,
                    ReviewerFullName = null
                });
            }
        }

        return reviews;
    }

    private static List<PostImageDetailResponse> MapImages(DataTable imagesTable)
    {
        var images = new List<PostImageDetailResponse>();

        // Return empty list if table is null or has no columns
        if (imagesTable == null || imagesTable.Columns.Count == 0)
            return images;

        // Return empty list if no rows
        if (imagesTable.Rows.Count == 0)
            return images;

        // Fail fast with clear message if wrong table was passed
        if (!imagesTable.Columns.Contains("PostImageID"))
        {
            var cols = string.Join(", ", imagesTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            throw new ArgumentException(
                $"MapImages received wrong DataTable. Expected Images result set with PostImageID column. " +
                $"Received columns: [{cols}]. This usually means the stored procedure result sets are being read incorrectly.");
        }

        foreach (DataRow row in imagesTable.Rows)
        {
            images.Add(new PostImageDetailResponse
            {
                PostImageID = row["PostImageID"] is int imageId ? imageId : 0,
                PostID = row["PostID"] is int postId ? postId : 0,
                PostImageURL = row["PostImageURL"] as string ?? string.Empty,
                UploadedAt = row["UploadedAt"] is DateTime uploadedAt ? uploadedAt : DateTime.MinValue
            });
        }

        return images;
    }
}
