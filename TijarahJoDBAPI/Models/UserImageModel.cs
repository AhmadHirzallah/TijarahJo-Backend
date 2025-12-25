namespace Models;

/// <summary>
/// Domain model representing a user profile image
/// </summary>
public class UserImageModel
{
    public UserImageModel(
        int? userImageId,
        int userId,
        string imageUrl,
        DateTime uploadedAt,
        bool isDeleted)
    {
        UserImageID = userImageId;
        UserID = userId;
        ImageURL = imageUrl;
        UploadedAt = uploadedAt;
        IsDeleted = isDeleted;
    }

    public int? UserImageID { get; set; }
    public int UserID { get; set; }
    public string ImageURL { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool IsDeleted { get; set; }
}
