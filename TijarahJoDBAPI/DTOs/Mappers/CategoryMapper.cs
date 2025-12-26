using Models;
using TijarahJoDBAPI.DTOs.Responses;

namespace TijarahJoDBAPI.DTOs.Mappers;

public static class CategoryMapper
{
    public static CategoryResponse ToResponse(this CategoryModel model)
    {
        return new CategoryResponse
        {
            CategoryID = model.CategoryID,
            CategoryName = model.CategoryName,
            CreatedAt = model.CreatedAt,
            IsDeleted = model.IsDeleted
        };
    }

    public static List<CategoryResponse> ToResponseList(this IEnumerable<CategoryModel> models)
    {
        return models.Select(m => m.ToResponse()).ToList();
    }
}
