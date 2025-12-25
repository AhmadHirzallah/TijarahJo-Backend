using System.Data;
using Models;
using TijarahJoDBAPI.DTOs.Responses;

namespace TijarahJoDBAPI.DTOs.Mappers;

/// <summary>
/// Mapper for UserImage entities
/// </summary>
public static class UserImageMapper
{
    /// <summary>
    /// Maps UserImageModel to UserImageResponse
    /// </summary>
    public static UserImageResponse ToResponse(this UserImageModel model)
    {
        return new UserImageResponse
        {
            UserImageID = model.UserImageID,
            UserID = model.UserID,
            ImageURL = model.ImageURL,
            UploadedAt = model.UploadedAt,
            IsDeleted = model.IsDeleted
        };
    }

    /// <summary>
    /// Maps a DataRow to UserImageResponse
    /// </summary>
    public static UserImageResponse ToUserImageResponse(this DataRow row)
    {
        return new UserImageResponse
        {
            UserImageID = row["UserImageID"] as int?,
            UserID = row["UserID"] is int userId ? userId : 0,
            ImageURL = row["ImageURL"] as string ?? string.Empty,
            UploadedAt = row["UploadedAt"] is DateTime uploadedAt ? uploadedAt : DateTime.MinValue,
            IsDeleted = row["IsDeleted"] is bool isDeleted && isDeleted
        };
    }

    /// <summary>
    /// Maps DataTable to list of UserImageResponse
    /// </summary>
    public static List<UserImageResponse> ToUserImageResponseList(this DataTable table)
    {
        var list = new List<UserImageResponse>();
        foreach (DataRow row in table.Rows)
        {
            list.Add(row.ToUserImageResponse());
        }
        return list;
    }
}
