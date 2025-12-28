using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;
using TijarahJoDB_DataAccess;

namespace TijarahJoDB.DAL;

/// <summary>
/// Data access layer for TbSystemSettings
/// </summary>
public class SystemSettingData
{
    /// <summary>
    /// Gets all system settings
    /// </summary>
    /// <param name="publicOnly">If true, only return settings marked as public</param>
    public static DataTable GetAllSettings(bool publicOnly = false)
    {
        var dt = new DataTable();

        try
        {
            using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
            using var command = new SqlCommand("SP_GetAllSettings", connection);
            
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@PublicOnly", publicOnly);

            connection.Open();

            using var reader = command.ExecuteReader();
            if (reader.HasRows)
                dt.Load(reader);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetAllSettings Error: {ex.Message}");
        }

        return dt;
    }

    /// <summary>
    /// Gets settings by group name
    /// </summary>
    public static DataTable GetSettingsByGroup(string settingGroup, bool publicOnly = false)
    {
        var dt = new DataTable();

        try
        {
            using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
            using var command = new SqlCommand("SP_GetSettingsByGroup", connection);
            
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@SettingGroup", settingGroup);
            command.Parameters.AddWithValue("@PublicOnly", publicOnly);

            connection.Open();

            using var reader = command.ExecuteReader();
            if (reader.HasRows)
                dt.Load(reader);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetSettingsByGroup Error: {ex.Message}");
        }

        return dt;
    }

    /// <summary>
    /// Gets a single setting by key
    /// </summary>
    public static SystemSettingModel? GetSettingByKey(string settingKey)
    {
        try
        {
            using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
            using var command = new SqlCommand("SP_GetSettingByKey", connection);
            
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@SettingKey", settingKey);

            connection.Open();

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new SystemSettingModel(
                    reader.GetInt32(reader.GetOrdinal("SettingID")),
                    reader.GetString(reader.GetOrdinal("SettingKey")),
                    reader.IsDBNull(reader.GetOrdinal("SettingValue")) ? null : reader.GetString(reader.GetOrdinal("SettingValue")),
                    reader.GetString(reader.GetOrdinal("SettingGroup")),
                    reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    reader.GetString(reader.GetOrdinal("DataType")),
                    reader.GetBoolean(reader.GetOrdinal("IsPublic")),
                    reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                    reader.IsDBNull(reader.GetOrdinal("UpdatedByUserID")) ? null : reader.GetInt32(reader.GetOrdinal("UpdatedByUserID"))
                );
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetSettingByKey Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets support contact information (email and WhatsApp)
    /// </summary>
    public static SupportContactModel GetSupportContactInfo()
    {
        var result = new SupportContactModel();

        try
        {
            using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
            using var command = new SqlCommand("SP_GetSupportContactInfo", connection);
            
            command.CommandType = CommandType.StoredProcedure;

            connection.Open();

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var key = reader.GetString(reader.GetOrdinal("SettingKey"));
                var value = reader.IsDBNull(reader.GetOrdinal("SettingValue")) 
                    ? null 
                    : reader.GetString(reader.GetOrdinal("SettingValue"));

                switch (key)
                {
                    case "SupportEmail":
                        result.SupportEmail = value;
                        break;
                    case "SupportWhatsApp":
                        result.SupportWhatsApp = value;
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetSupportContactInfo Error: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Updates a single setting by key
    /// </summary>
    public static bool UpdateSettingByKey(string settingKey, string? settingValue, int? updatedByUserId = null)
    {
        try
        {
            using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
            using var command = new SqlCommand("SP_UpdateSettingByKey", connection);
            
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@SettingKey", settingKey);
            command.Parameters.AddWithValue("@SettingValue", (object?)settingValue ?? DBNull.Value);
            command.Parameters.AddWithValue("@UpdatedByUserID", (object?)updatedByUserId ?? DBNull.Value);

            connection.Open();

            var result = command.ExecuteScalar();
            return result != null && Convert.ToInt32(result) > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateSettingByKey Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Updates multiple settings at once (batch update)
    /// </summary>
    /// <param name="settingsJson">JSON array: [{"key":"Key1","value":"Value1"},...]</param>
    public static bool UpdateSettings(string settingsJson, int? updatedByUserId = null)
    {
        try
        {
            using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
            using var command = new SqlCommand("SP_UpdateSettings", connection);
            
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@SettingsJson", settingsJson);
            command.Parameters.AddWithValue("@UpdatedByUserID", (object?)updatedByUserId ?? DBNull.Value);

            connection.Open();

            var result = command.ExecuteScalar();
            return result != null && Convert.ToInt32(result) > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateSettings Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Creates a new setting
    /// </summary>
    public static int CreateSetting(SystemSettingModel setting, int? createdByUserId = null)
    {
        try
        {
            using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
            using var command = new SqlCommand("SP_CreateSetting", connection);
            
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@SettingKey", setting.SettingKey);
            command.Parameters.AddWithValue("@SettingValue", (object?)setting.SettingValue ?? DBNull.Value);
            command.Parameters.AddWithValue("@SettingGroup", setting.SettingGroup);
            command.Parameters.AddWithValue("@Description", (object?)setting.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@DataType", setting.DataType);
            command.Parameters.AddWithValue("@IsPublic", setting.IsPublic);
            command.Parameters.AddWithValue("@CreatedByUserID", (object?)createdByUserId ?? DBNull.Value);

            connection.Open();

            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateSetting Error: {ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// Checks if a setting key exists
    /// </summary>
    public static bool DoesSettingExist(string settingKey)
    {
        try
        {
            using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
            using var command = new SqlCommand("SP_DoesSettingExist", connection);
            
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@SettingKey", settingKey);

            connection.Open();

            var result = command.ExecuteScalar();
            return result != null && (bool)result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DoesSettingExist Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Deletes a setting by key
    /// </summary>
    public static bool DeleteSetting(string settingKey)
    {
        try
        {
            using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
            using var command = new SqlCommand("SP_DeleteSetting", connection);
            
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@SettingKey", settingKey);

            connection.Open();

            var result = command.ExecuteScalar();
            return result != null && Convert.ToInt32(result) > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DeleteSetting Error: {ex.Message}");
            return false;
        }
    }
}
