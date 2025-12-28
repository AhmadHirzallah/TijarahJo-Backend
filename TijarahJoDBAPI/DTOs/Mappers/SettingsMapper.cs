using System.Data;
using Models;
using TijarahJoDB.BLL;
using TijarahJoDBAPI.DTOs.Responses;

namespace TijarahJoDBAPI.DTOs.Mappers;

/// <summary>
/// Mapper for System Settings
/// </summary>
public static class SettingsMapper
{
    /// <summary>
    /// Maps SystemSettingModel to SettingResponse
    /// </summary>
    public static SettingResponse ToResponse(this SystemSettingModel model)
    {
        return new SettingResponse
        {
            SettingID = model.SettingID ?? 0,
            SettingKey = model.SettingKey,
            SettingValue = model.SettingValue,
            SettingGroup = model.SettingGroup,
            Description = model.Description,
            DataType = model.DataType,
            IsPublic = model.IsPublic,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            UpdatedByUserID = model.UpdatedByUserID
        };
    }

    /// <summary>
    /// Maps SystemSettingBL to SettingResponse
    /// </summary>
    public static SettingResponse ToResponse(this SystemSettingBL setting)
    {
        return setting.SettingModel.ToResponse();
    }

    /// <summary>
    /// Maps DataTable to list of SettingResponse
    /// </summary>
    public static List<SettingResponse> ToSettingResponseList(this DataTable dt)
    {
        var list = new List<SettingResponse>();

        foreach (DataRow row in dt.Rows)
        {
            list.Add(new SettingResponse
            {
                SettingID = (int)row["SettingID"],
                SettingKey = (string)row["SettingKey"],
                SettingValue = row["SettingValue"] == DBNull.Value ? null : (string)row["SettingValue"],
                SettingGroup = (string)row["SettingGroup"],
                Description = row["Description"] == DBNull.Value ? null : (string)row["Description"],
                DataType = (string)row["DataType"],
                IsPublic = (bool)row["IsPublic"],
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = (DateTime)row["UpdatedAt"],
                UpdatedByUserID = row["UpdatedByUserID"] == DBNull.Value ? null : (int?)row["UpdatedByUserID"]
            });
        }

        return list;
    }

    /// <summary>
    /// Maps DataTable to AllSettingsResponse with group counts
    /// </summary>
    public static AllSettingsResponse ToAllSettingsResponse(this DataTable dt)
    {
        var settings = dt.ToSettingResponseList();
        
        return new AllSettingsResponse
        {
            Settings = settings,
            GroupCounts = settings
                .GroupBy(s => s.SettingGroup)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    /// <summary>
    /// Maps SupportContactModel to SupportContactResponse
    /// </summary>
    public static SupportContactResponse ToResponse(this SupportContactModel model)
    {
        return new SupportContactResponse
        {
            SupportEmail = model.SupportEmail,
            SupportWhatsApp = model.SupportWhatsApp,
            WhatsAppLink = model.WhatsAppLink
        };
    }

    /// <summary>
    /// Groups settings by their group name
    /// </summary>
    public static List<SettingsGroupResponse> ToGroupedResponse(this DataTable dt)
    {
        var settings = dt.ToSettingResponseList();
        
        return settings
            .GroupBy(s => s.SettingGroup)
            .Select(g => new SettingsGroupResponse
            {
                GroupName = g.Key,
                Settings = g.ToList()
            })
            .OrderBy(g => g.GroupName)
            .ToList();
    }
}
