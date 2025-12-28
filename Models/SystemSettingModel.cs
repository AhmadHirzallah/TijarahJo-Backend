namespace Models;

/// <summary>
/// Domain model representing a system setting
/// </summary>
public class SystemSettingModel
{
    public SystemSettingModel(
        int? settingId,
        string settingKey,
        string? settingValue,
        string settingGroup,
        string? description,
        string dataType,
        bool isPublic,
        DateTime createdAt,
        DateTime updatedAt,
        int? updatedByUserId)
    {
        SettingID = settingId;
        SettingKey = settingKey;
        SettingValue = settingValue;
        SettingGroup = settingGroup;
        Description = description;
        DataType = dataType;
        IsPublic = isPublic;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        UpdatedByUserID = updatedByUserId;
    }

    public int? SettingID { get; set; }
    public string SettingKey { get; set; }
    public string? SettingValue { get; set; }
    public string SettingGroup { get; set; }
    public string? Description { get; set; }
    public string DataType { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? UpdatedByUserID { get; set; }
}

/// <summary>
/// Simplified model for support contact info
/// </summary>
public class SupportContactModel
{
    public string? SupportEmail { get; set; }
    public string? SupportWhatsApp { get; set; }
    
    /// <summary>
    /// Pre-computed WhatsApp link
    /// </summary>
    public string? WhatsAppLink => !string.IsNullOrEmpty(SupportWhatsApp) 
        ? $"https://wa.me/{SupportWhatsApp}" 
        : null;
}
