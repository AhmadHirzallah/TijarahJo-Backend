using System;

namespace Models;

/// <summary>
/// Represents a user phone number (One-to-Many: User -> PhoneNumbers)
/// </summary>
public class UserPhoneNumberModel
{
    public UserPhoneNumberModel(
        int? phoneId,
        int userId,
        string phoneNumber,
        bool isPrimary,
        DateTime createdAt,
        bool isDeleted)
    {
        PhoneID = phoneId;
        UserID = userId;
        PhoneNumber = phoneNumber;
        IsPrimary = isPrimary;
        CreatedAt = createdAt;
        IsDeleted = isDeleted;
    }

    public int? PhoneID { get; set; }
    public int UserID { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
