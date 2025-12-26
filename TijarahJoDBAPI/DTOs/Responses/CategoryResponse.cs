namespace TijarahJoDBAPI.DTOs.Responses;

public class CategoryResponse
{
    public int? CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
