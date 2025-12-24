using Models;
using TijarahJoDBAPI.DTOs.Responses;

namespace TijarahJoDBAPI.DTOs.Mappers;

public static class RoleMapper
{
    public static RoleResponse ToResponse(this RoleModel model)
    {
        return new RoleResponse
        {
            RoleID = model.RoleID,
            RoleName = model.RoleName,
            CreatedAt = model.CreatedAt,
            IsDeleted = model.IsDeleted
        };
    }

    public static List<RoleResponse> ToResponseList(this IEnumerable<RoleModel> models)
    {
        return models.Select(m => m.ToResponse()).ToList();
    }
}
