using System.ComponentModel.DataAnnotations;

namespace TijarahJoDBAPI.DTOs.Requests;

/// <summary>
/// Request to update support contact settings
/// </summary>
public class UpdateSupportContactRequest
{
    /// <summary>
    /// Support email address
    /// </summary>
    [Required(ErrorMessage = "Support email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
    public string SupportEmail { get; set; } = string.Empty;

    /// <summary>
    /// Support WhatsApp number (Jordan format: 9627XXXXXXXX - 12 digits)
    /// </summary>
    [Required(ErrorMessage = "WhatsApp number is required.")]
    [RegularExpression(@"^9627[789]\d{7}$", ErrorMessage = "WhatsApp number must be in Jordan format: 9627[7/8/9]XXXXXXX (12 digits)")]
    public string SupportWhatsApp { get; set; } = string.Empty;
}

/// <summary>
/// Request to update a single setting
/// </summary>
public class UpdateSettingRequest
{
    /// <summary>
    /// Setting key to update
    /// </summary>
    [Required(ErrorMessage = "Setting key is required.")]
    [StringLength(100, ErrorMessage = "Setting key cannot exceed 100 characters.")]
    public string SettingKey { get; set; } = string.Empty;

    /// <summary>
    /// New value for the setting
    /// </summary>
    [StringLength(500, ErrorMessage = "Setting value cannot exceed 500 characters.")]
    public string? SettingValue { get; set; }
}

/// <summary>
/// Request to update multiple settings at once
/// </summary>
public class UpdateSettingsBatchRequest
{
    /// <summary>
    /// List of settings to update
    /// </summary>
    [Required(ErrorMessage = "Settings list is required.")]
    [MinLength(1, ErrorMessage = "At least one setting is required.")]
    public List<UpdateSettingRequest> Settings { get; set; } = new();
}

/// <summary>
/// Request to create a new setting (Admin only)
/// </summary>
public class CreateSettingRequest
{
    /// <summary>
    /// Unique key for the setting
    /// </summary>
    [Required(ErrorMessage = "Setting key is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Setting key must be between 2 and 100 characters.")]
    [RegularExpression(@"^[A-Za-z][A-Za-z0-9_]*$", ErrorMessage = "Setting key must start with a letter and contain only letters, numbers, and underscores.")]
    public string SettingKey { get; set; } = string.Empty;

    /// <summary>
    /// Value for the setting
    /// </summary>
    [StringLength(500, ErrorMessage = "Setting value cannot exceed 500 characters.")]
    public string? SettingValue { get; set; }

    /// <summary>
    /// Group for organizing settings (e.g., "General", "Support", "Upload")
    /// </summary>
    [Required(ErrorMessage = "Setting group is required.")]
    [StringLength(50, ErrorMessage = "Setting group cannot exceed 50 characters.")]
    public string SettingGroup { get; set; } = "General";

    /// <summary>
    /// Description of what this setting does
    /// </summary>
    [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// Data type of the setting value (String, Int, Bool, Json)
    /// </summary>
    [Required(ErrorMessage = "Data type is required.")]
    [RegularExpression(@"^(String|Int|Bool|Json)$", ErrorMessage = "DataType must be one of: String, Int, Bool, Json")]
    public string DataType { get; set; } = "String";

    /// <summary>
    /// Whether this setting can be read without authentication
    /// </summary>
    public bool IsPublic { get; set; } = false;
}
