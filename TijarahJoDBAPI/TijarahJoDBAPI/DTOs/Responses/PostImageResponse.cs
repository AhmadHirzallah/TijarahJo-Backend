namespace TijarahJoDBAPI.DTOs.Responses;

public class PostImageResponse
{
    public int? PostImageID { get; set; }
    public int PostID { get; set; }
    public string PostImageURL { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public bool IsDeleted { get; set; }
}
