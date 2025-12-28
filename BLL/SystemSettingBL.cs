using System.Data;
using System.Text.Json;
using Models;
using TijarahJoDB.DAL;

namespace TijarahJoDB.BLL;

/// <summary>
/// Business logic layer for System Settings
/// </summary>
public class SystemSettingBL
{
    public enum enMode { AddNew = 0, Update = 1 }
    public enMode Mode = enMode.AddNew;

    public SystemSettingModel SettingModel => new SystemSettingModel(
        SettingID, SettingKey, SettingValue, SettingGroup, 
        Description, DataType, IsPublic, CreatedAt, UpdatedAt, UpdatedByUserID);

    public int? SettingID { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string? SettingValue { get; set; }
    public string SettingGroup { get; set; } = "General";
    public string? Description { get; set; }
    public string DataType { get; set; } = "String";
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? UpdatedByUserID { get; set; }

    public SystemSettingBL(SystemSettingModel model, enMode mode = enMode.AddNew)
    {
        SettingID = model.SettingID;
        SettingKey = model.SettingKey;
        SettingValue = model.SettingValue;
        SettingGroup = model.SettingGroup;
        Description = model.Description;
        DataType = model.DataType;
        IsPublic = model.IsPublic;
        CreatedAt = model.CreatedAt;
        UpdatedAt = model.UpdatedAt;
        UpdatedByUserID = model.UpdatedByUserID;
        Mode = mode;
    }

    #region Static Methods

    /// <summary>
    /// Gets all system settings
    /// </summary>
    public static DataTable GetAllSettings(bool publicOnly = false)
        => SystemSettingData.GetAllSettings(publicOnly);

    /// <summary>
    /// Gets settings by group
    /// </summary>
    public static DataTable GetSettingsByGroup(string group, bool publicOnly = false)
        => SystemSettingData.GetSettingsByGroup(group, publicOnly);

    /// <summary>
    /// Finds a setting by key
    /// </summary>
    public static SystemSettingBL? FindByKey(string settingKey)
    {
        var model = SystemSettingData.GetSettingByKey(settingKey);
        return model != null ? new SystemSettingBL(model, enMode.Update) : null;
    }

    /// <summary>
    /// Gets the value of a setting by key
    /// </summary>
    public static string? GetValue(string settingKey)
    {
        var model = SystemSettingData.GetSettingByKey(settingKey);
        return model?.SettingValue;
    }

    /// <summary>
    /// Gets the value of a setting as int
    /// </summary>
    public static int GetValueAsInt(string settingKey, int defaultValue = 0)
    {
        var value = GetValue(settingKey);
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Gets the value of a setting as bool
    /// </summary>
    public static bool GetValueAsBool(string settingKey, bool defaultValue = false)
    {
        var value = GetValue(settingKey);
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Gets support contact information
    /// </summary>
    public static SupportContactModel GetSupportContactInfo()
        => SystemSettingData.GetSupportContactInfo();

    /// <summary>
    /// Updates a single setting value
    /// </summary>
    public static bool UpdateValue(string settingKey, string? value, int? updatedByUserId = null)
        => SystemSettingData.UpdateSettingByKey(settingKey, value, updatedByUserId);

    /// <summary>
    /// Updates multiple settings at once
    /// </summary>
    public static bool UpdateMultiple(Dictionary<string, string?> settings, int? updatedByUserId = null)
    {
        var jsonArray = settings.Select(kv => new { key = kv.Key, value = kv.Value }).ToList();
        var json = JsonSerializer.Serialize(jsonArray);
        return SystemSettingData.UpdateSettings(json, updatedByUserId);
    }

    /// <summary>
    /// Updates support contact settings
    /// </summary>
    public static bool UpdateSupportContact(string? email, string? whatsApp, int? updatedByUserId = null)
    {
        var settings = new Dictionary<string, string?>
        {
            { "SupportEmail", email },
            { "SupportWhatsApp", whatsApp }
        };
        return UpdateMultiple(settings, updatedByUserId);
    }

    /// <summary>
    /// Checks if a setting exists
    /// </summary>
    public static bool DoesExist(string settingKey)
        => SystemSettingData.DoesSettingExist(settingKey);

    /// <summary>
    /// Deletes a setting
    /// </summary>
    public static bool Delete(string settingKey)
        => SystemSettingData.DeleteSetting(settingKey);

    #endregion

    #region Instance Methods

    private bool _AddNew(int? createdByUserId)
    {
        SettingID = SystemSettingData.CreateSetting(SettingModel, createdByUserId);
        return SettingID != -1;
    }

    private bool _Update(int? updatedByUserId)
    {
        return SystemSettingData.UpdateSettingByKey(SettingKey, SettingValue, updatedByUserId);
    }

    public bool Save(int? userId = null)
    {
        switch (Mode)
        {
            case enMode.AddNew:
                if (_AddNew(userId))
                {
                    Mode = enMode.Update;
                    return true;
                }
                return false;

            case enMode.Update:
                return _Update(userId);
        }
        return false;
    }

    #endregion
}

/// <summary>
/// Well-known setting keys
/// </summary>
public static class SettingKeys
{
    public const string SupportEmail = "SupportEmail";
    public const string SupportWhatsApp = "SupportWhatsApp";
    public const string SiteName = "SiteName";
    public const string MaintenanceMode = "MaintenanceMode";
    public const string MaxUploadSizeMB = "MaxUploadSizeMB";
}

/// <summary>
/// Setting groups
/// </summary>
public static class SettingGroups
{
    public const string General = "General";
    public const string Support = "Support";
    public const string Upload = "Upload";
}
