using Models;
using TijarahJoDBAPI.DTOs.Responses;
using System.Data;

namespace TijarahJoDBAPI.DTOs.Mappers;

public static class UserPhoneMapper
{
    public static UserPhoneResponse ToResponse(this UserPhoneNumberModel model)
    {
        return new UserPhoneResponse
        {
            PhoneID = model.PhoneID,
            UserID = model.UserID,
            PhoneNumber = model.PhoneNumber,
            IsPrimary = model.IsPrimary,
            CreatedAt = model.CreatedAt,
            IsDeleted = model.IsDeleted
        };
    }

    public static List<UserPhoneResponse> ToUserPhoneResponseList(this DataTable dt)
    {
        var list = new List<UserPhoneResponse>();

        foreach (DataRow row in dt.Rows)
        {
            list.Add(new UserPhoneResponse
            {
                PhoneID = (int)row["PhoneID"],
                UserID = (int)row["UserID"],
                PhoneNumber = (string)row["PhoneNumber"],
                IsPrimary = (bool)row["IsPrimary"],
                CreatedAt = (DateTime)row["CreatedAt"],
                IsDeleted = (bool)row["IsDeleted"]
            });
        }

        return list;
    }
}
