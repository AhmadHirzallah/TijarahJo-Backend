namespace TijarahJoDBAPI.DTOs.Responses;

public class PostResponse
{
    public int? PostID { get; set; }
    public int UserID { get; set; }
    public int CategoryID { get; set; }
    public string PostTitle { get; set; } = string.Empty;
    public string? PostDescription { get; set; }
    public decimal? Price { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
