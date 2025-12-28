namespace TijarahJoDBAPI.DTOs.Responses;

/// <summary>
/// Response for a single system setting
/// </summary>
public class SettingResponse
{
    public int SettingID { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string? SettingValue { get; set; }
    public string SettingGroup { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DataType { get; set; } = "String";
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? UpdatedByUserID { get; set; }
}

/// <summary>
/// Response for all settings
/// </summary>
public class AllSettingsResponse
{
    public List<SettingResponse> Settings { get; set; } = new();
    public int TotalCount => Settings.Count;
    public Dictionary<string, int> GroupCounts { get; set; } = new();
}

/// <summary>
/// Response for settings grouped by category
/// </summary>
public class SettingsGroupResponse
{
    public string GroupName { get; set; } = string.Empty;
    public List<SettingResponse> Settings { get; set; } = new();
    public int Count => Settings.Count;
}

/// <summary>
/// Response for support contact information (public endpoint)
/// </summary>
public class SupportContactResponse
{
    /// <summary>
    /// Support email address
    /// </summary>
    public string? SupportEmail { get; set; }

    /// <summary>
    /// Support WhatsApp number
    /// </summary>
    public string? SupportWhatsApp { get; set; }

    /// <summary>
    /// Pre-computed WhatsApp link for easy use
    /// </summary>
    public string? WhatsAppLink { get; set; }

    /// <summary>
    /// Mailto link for email
    /// </summary>
    public string? EmailLink => !string.IsNullOrEmpty(SupportEmail) 
        ? $"mailto:{SupportEmail}" 
        : null;

    /// <summary>
    /// Indicates if support contact info is configured
    /// </summary>
    public bool IsConfigured => !string.IsNullOrEmpty(SupportEmail) || !string.IsNullOrEmpty(SupportWhatsApp);

    /// <summary>
    /// WhatsApp link with pre-filled support message
    /// </summary>
    public string? WhatsAppSupportLink => !string.IsNullOrEmpty(SupportWhatsApp)
        ? $"https://wa.me/{SupportWhatsApp}?text={Uri.EscapeDataString("Hi, I need help with TijarahJo.")}"
        : null;
}

/// <summary>
/// Response after updating settings
/// </summary>
public class SettingsUpdateResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int UpdatedCount { get; set; }
}
