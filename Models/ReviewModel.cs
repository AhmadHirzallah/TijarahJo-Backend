namespace Models;

/// <summary>
/// Domain model representing a post review
/// </summary>
public class ReviewModel
{
    public ReviewModel(
        int? reviewId,
        int postId,
        int userId,
        byte rating,
        string? reviewText,
        DateTime createdAt,
        bool isDeleted)
    {
        ReviewID = reviewId;
        PostID = postId;
        UserID = userId;
        Rating = rating;
        ReviewText = reviewText;
        CreatedAt = createdAt;
        IsDeleted = isDeleted;
    }

    public int? ReviewID { get; set; }
    public int PostID { get; set; }
    public int UserID { get; set; }
    public byte Rating { get; set; }
    public string? ReviewText { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
