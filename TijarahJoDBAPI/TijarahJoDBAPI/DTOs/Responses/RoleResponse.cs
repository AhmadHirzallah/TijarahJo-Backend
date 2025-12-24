namespace TijarahJoDBAPI.DTOs.Responses;

public class RoleResponse
{
    public int? RoleID { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
