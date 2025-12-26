using System.Data;
using Models;
using TijarahJoDBAPI.DTOs.Responses;

namespace TijarahJoDBAPI.DTOs.Mappers;

/// <summary>
/// Mapper for Review entities
/// </summary>
public static class ReviewMapper
{
    /// <summary>
    /// Maps ReviewModel to ReviewResponse
    /// </summary>
    public static ReviewResponse ToResponse(this ReviewModel model)
    {
        return new ReviewResponse
        {
            ReviewID = model.ReviewID,
            PostID = model.PostID,
            UserID = model.UserID,
            Rating = model.Rating,
            ReviewText = model.ReviewText,
            CreatedAt = model.CreatedAt,
            IsDeleted = model.IsDeleted
        };
    }

    /// <summary>
    /// Maps a DataRow to ReviewResponse (basic)
    /// </summary>
    public static ReviewResponse ToReviewResponse(this DataRow row)
    {
        return new ReviewResponse
        {
            ReviewID = row["ReviewID"] as int?,
            PostID = row["PostID"] is int postId ? postId : 0,
            UserID = row["UserID"] is int userId ? userId : 0,
            Rating = row["Rating"] is byte rating ? rating : (byte)0,
            ReviewText = row["ReviewText"] == DBNull.Value ? null : row["ReviewText"] as string,
            CreatedAt = row["CreatedAt"] is DateTime createdAt ? createdAt : DateTime.MinValue,
            IsDeleted = row["IsDeleted"] is bool isDeleted && isDeleted
        };
    }

    /// <summary>
    /// Maps a DataRow to ReviewDetailResponse (enriched with reviewer info)
    /// </summary>
    public static ReviewDetailResponse ToDetailResponse(this DataRow row)
    {
        return new ReviewDetailResponse
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
            ReviewerLastName = row["ReviewerLastName"] == DBNull.Value ? null : row["ReviewerLastName"] as string,
            ReviewerFullName = row["ReviewerFullName"] as string
        };
    }

    /// <summary>
    /// Maps DataTable to list of ReviewResponse
    /// </summary>
    public static List<ReviewResponse> ToResponseList(this DataTable table)
    {
        var list = new List<ReviewResponse>();
        foreach (DataRow row in table.Rows)
        {
            list.Add(row.ToReviewResponse());
        }
        return list;
    }

    /// <summary>
    /// Maps DataTable to list of ReviewDetailResponse
    /// </summary>
    public static List<ReviewDetailResponse> ToDetailResponseList(this DataTable table)
    {
        var list = new List<ReviewDetailResponse>();
        foreach (DataRow row in table.Rows)
        {
            list.Add(row.ToDetailResponse());
        }
        return list;
    }
}
