namespace TijarahJoDBAPI.DTOs.Responses;

/// <summary>
/// Response for a single user phone number
/// </summary>
public class UserPhoneResponse
{
    public int? PhoneID { get; set; }
    public int UserID { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Response containing all phone numbers for a user
/// </summary>
public class UserPhonesResponse
{
    public int UserID { get; set; }
    public List<UserPhoneResponse> PhoneNumbers { get; set; } = new();
    public int TotalCount { get; set; }
    public string? PrimaryPhone => PhoneNumbers.FirstOrDefault(p => p.IsPrimary)?.PhoneNumber;
}
