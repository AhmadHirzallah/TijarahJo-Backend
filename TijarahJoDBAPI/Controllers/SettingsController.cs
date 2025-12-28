using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using TijarahJoDB.BLL;
using TijarahJoDBAPI.DTOs.Requests;
using TijarahJoDBAPI.DTOs.Responses;
using TijarahJoDBAPI.DTOs.Mappers;
using TijarahJoDBAPI.Extensions;

namespace TijarahJoDBAPI.Controllers;

/// <summary>
/// Controller for system settings management.
/// Public endpoints for support contact info, admin endpoints for full management.
/// </summary>
[ApiController]
[Route("api/settings")]
[Produces("application/json")]
public class SettingsController : ControllerBase
{
    #region ==================== PUBLIC ENDPOINTS ====================

    /// <summary>
    /// Gets support contact information (public)
    /// </summary>
    /// <remarks>
    /// Returns support email and WhatsApp number for contact pages.
    /// This endpoint is public and does not require authentication.
    /// </remarks>
    [HttpGet("support")]
    [EndpointSummary("Gets support contact info")]
    [EndpointDescription("Returns support email and WhatsApp number. Public endpoint - no authentication required.")]
    [ProducesResponseType(typeof(SupportContactResponse), StatusCodes.Status200OK)]
    public ActionResult<SupportContactResponse> GetSupportContact()
    {
        var supportInfo = SystemSettingBL.GetSupportContactInfo();
        return Ok(supportInfo.ToResponse());
    }

    /// <summary>
    /// Gets all public settings
    /// </summary>
    /// <remarks>
    /// Returns settings that are marked as public (IsPublic = true).
    /// Useful for frontend configuration without authentication.
    /// </remarks>
    [HttpGet("public")]
    [EndpointSummary("Gets all public settings")]
    [EndpointDescription("Returns settings marked as public. No authentication required.")]
    [ProducesResponseType(typeof(AllSettingsResponse), StatusCodes.Status200OK)]
    public ActionResult<AllSettingsResponse> GetPublicSettings()
    {
        var settingsTable = SystemSettingBL.GetAllSettings(publicOnly: true);
        return Ok(settingsTable.ToAllSettingsResponse());
    }

    /// <summary>
    /// Gets a specific public setting by key
    /// </summary>
    [HttpGet("public/{key}")]
    [EndpointSummary("Gets a public setting by key")]
    [ProducesResponseType(typeof(SettingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<SettingResponse> GetPublicSetting(string key)
    {
        var setting = SystemSettingBL.FindByKey(key);
        
        if (setting == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Setting Not Found",
                Detail = $"No setting found with key '{key}'.",
                Status = StatusCodes.Status404NotFound
            });
        }

        // Only return if public
        if (!setting.IsPublic)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Setting Not Found",
                Detail = $"No public setting found with key '{key}'.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(setting.ToResponse());
    }

    #endregion

    #region ==================== ADMIN ENDPOINTS ====================

    /// <summary>
    /// Gets all settings (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    [EndpointSummary("Gets all settings (Admin)")]
    [EndpointDescription("Returns all system settings including private ones. Admin only.")]
    [ProducesResponseType(typeof(AllSettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<AllSettingsResponse> GetAllSettings()
    {
        var settingsTable = SystemSettingBL.GetAllSettings(publicOnly: false);
        return Ok(settingsTable.ToAllSettingsResponse());
    }

    /// <summary>
    /// Gets settings grouped by category (Admin only)
    /// </summary>
    [HttpGet("grouped")]
    [Authorize(Roles = RoleNames.Admin)]
    [EndpointSummary("Gets settings grouped by category (Admin)")]
    [ProducesResponseType(typeof(List<SettingsGroupResponse>), StatusCodes.Status200OK)]
    public ActionResult<List<SettingsGroupResponse>> GetSettingsGrouped()
    {
        var settingsTable = SystemSettingBL.GetAllSettings(publicOnly: false);
        return Ok(settingsTable.ToGroupedResponse());
    }

    /// <summary>
    /// Gets settings by group (Admin only)
    /// </summary>
    [HttpGet("group/{groupName}")]
    [Authorize(Roles = RoleNames.Admin)]
    [EndpointSummary("Gets settings by group (Admin)")]
    [ProducesResponseType(typeof(SettingsGroupResponse), StatusCodes.Status200OK)]
    public ActionResult<SettingsGroupResponse> GetSettingsByGroup(string groupName)
    {
        var settingsTable = SystemSettingBL.GetSettingsByGroup(groupName, publicOnly: false);
        var settings = settingsTable.ToSettingResponseList();

        return Ok(new SettingsGroupResponse
        {
            GroupName = groupName,
            Settings = settings
        });
    }

    /// <summary>
    /// Gets a specific setting by key (Admin only)
    /// </summary>
    [HttpGet("{key}")]
    [Authorize(Roles = RoleNames.Admin)]
    [EndpointSummary("Gets a setting by key (Admin)")]
    [ProducesResponseType(typeof(SettingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<SettingResponse> GetSetting(string key)
    {
        var setting = SystemSettingBL.FindByKey(key);
        
        if (setting == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Setting Not Found",
                Detail = $"No setting found with key '{key}'.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(setting.ToResponse());
    }

    /// <summary>
    /// Updates support contact settings (Admin only)
    /// </summary>
    /// <remarks>
    /// Updates both SupportEmail and SupportWhatsApp settings at once.
    /// Use this endpoint from the admin settings page.
    /// </remarks>
    [HttpPut("support")]
    [Authorize(Roles = RoleNames.Admin)]
    [EndpointSummary("Updates support contact settings (Admin)")]
    [EndpointDescription("Updates support email and WhatsApp number. Admin only.")]
    [ProducesResponseType(typeof(SettingsUpdateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<SettingsUpdateResponse> UpdateSupportContact([FromBody] UpdateSupportContactRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var adminId = User.GetUserId();

        var success = SystemSettingBL.UpdateSupportContact(
            request.SupportEmail,
            request.SupportWhatsApp,
            adminId
        );

        if (!success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Update Failed",
                Detail = "Failed to update support contact settings.",
                Status = StatusCodes.Status500InternalServerError
            });
        }

        return Ok(new SettingsUpdateResponse
        {
            Success = true,
            Message = "Support contact settings updated successfully.",
            UpdatedAt = DateTime.UtcNow,
            UpdatedCount = 2
        });
    }

    /// <summary>
    /// Updates a single setting (Admin only)
    /// </summary>
    [HttpPut("{key}")]
    [Authorize(Roles = RoleNames.Admin)]
    [EndpointSummary("Updates a single setting (Admin)")]
    [ProducesResponseType(typeof(SettingsUpdateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<SettingsUpdateResponse> UpdateSetting(string key, [FromBody] UpdateSettingRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!SystemSettingBL.DoesExist(key))
        {
            return NotFound(new ProblemDetails
            {
                Title = "Setting Not Found",
                Detail = $"No setting found with key '{key}'.",
                Status = StatusCodes.Status404NotFound
            });
        }

        var adminId = User.GetUserId();
        var success = SystemSettingBL.UpdateValue(key, request.SettingValue, adminId);

        if (!success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Update Failed",
                Detail = $"Failed to update setting '{key}'.",
                Status = StatusCodes.Status500InternalServerError
            });
        }

        return Ok(new SettingsUpdateResponse
        {
            Success = true,
            Message = $"Setting '{key}' updated successfully.",
            UpdatedAt = DateTime.UtcNow,
            UpdatedCount = 1
        });
    }

    /// <summary>
    /// Updates multiple settings at once (Admin only)
    /// </summary>
    [HttpPut("batch")]
    [Authorize(Roles = RoleNames.Admin)]
    [EndpointSummary("Updates multiple settings (Admin)")]
    [EndpointDescription("Updates multiple settings in a single transaction. Admin only.")]
    [ProducesResponseType(typeof(SettingsUpdateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<SettingsUpdateResponse> UpdateSettingsBatch([FromBody] UpdateSettingsBatchRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var adminId = User.GetUserId();
        var settingsDict = request.Settings.ToDictionary(s => s.SettingKey, s => s.SettingValue);

        var success = SystemSettingBL.UpdateMultiple(settingsDict, adminId);

        if (!success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Update Failed",
                Detail = "Failed to update settings.",
                Status = StatusCodes.Status500InternalServerError
            });
        }

        return Ok(new SettingsUpdateResponse
        {
            Success = true,
            Message = $"Successfully updated {request.Settings.Count} settings.",
            UpdatedAt = DateTime.UtcNow,
            UpdatedCount = request.Settings.Count
        });
    }

    /// <summary>
    /// Creates a new setting (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    [EndpointSummary("Creates a new setting (Admin)")]
    [EndpointDescription("Creates a new system setting. Setting keys must be unique. Admin only.")]
    [ProducesResponseType(typeof(SettingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<SettingResponse> CreateSetting([FromBody] CreateSettingRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check if key already exists
        if (SystemSettingBL.DoesExist(request.SettingKey))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Setting Already Exists",
                Detail = $"A setting with key '{request.SettingKey}' already exists.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var adminId = User.GetUserId();

        var setting = new SystemSettingBL(new SystemSettingModel(
            null,
            request.SettingKey,
            request.SettingValue,
            request.SettingGroup,
            request.Description,
            request.DataType,
            request.IsPublic,
            DateTime.UtcNow,
            DateTime.UtcNow,
            adminId
        ));

        if (!setting.Save(adminId))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Creation Failed",
                Detail = "Failed to create setting.",
                Status = StatusCodes.Status500InternalServerError
            });
        }

        return CreatedAtAction(
            nameof(GetSetting),
            new { key = setting.SettingKey },
            setting.ToResponse()
        );
    }

    /// <summary>
    /// Deletes a setting (Admin only)
    /// </summary>
    /// <remarks>
    /// WARNING: This permanently deletes a setting. Use with caution.
    /// Core settings like SupportEmail cannot be deleted.
    /// </remarks>
    [HttpDelete("{key}")]
    [Authorize(Roles = RoleNames.Admin)]
    [EndpointSummary("Deletes a setting (Admin)")]
    [EndpointDescription("Permanently deletes a setting. Use with caution. Admin only.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult DeleteSetting(string key)
    {
        // Prevent deletion of core settings
        var protectedKeys = new[] { SettingKeys.SupportEmail, SettingKeys.SupportWhatsApp, SettingKeys.SiteName };
        if (protectedKeys.Contains(key))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Operation Not Allowed",
                Detail = $"Cannot delete protected setting '{key}'.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!SystemSettingBL.DoesExist(key))
        {
            return NotFound(new ProblemDetails
            {
                Title = "Setting Not Found",
                Detail = $"No setting found with key '{key}'.",
                Status = StatusCodes.Status404NotFound
            });
        }

        if (!SystemSettingBL.Delete(key))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Delete Failed",
                Detail = $"Failed to delete setting '{key}'.",
                Status = StatusCodes.Status500InternalServerError
            });
        }

        return NoContent();
    }

    #endregion
}
