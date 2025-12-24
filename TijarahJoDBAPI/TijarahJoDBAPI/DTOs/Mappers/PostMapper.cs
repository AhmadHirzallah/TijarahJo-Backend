using Models;
using TijarahJoDBAPI.DTOs.Responses;

namespace TijarahJoDBAPI.DTOs.Mappers;

public static class PostMapper
{
    public static PostResponse ToResponse(this PostModel model)
    {
        return new PostResponse
        {
            PostID = model.PostID,
            UserID = model.UserID,
            CategoryID = model.CategoryID,
            PostTitle = model.PostTitle,
            PostDescription = model.PostDescription,
            Price = model.Price,
            Status = model.Status,
            CreatedAt = model.CreatedAt,
            IsDeleted = model.IsDeleted
        };
    }

    public static List<PostResponse> ToResponseList(this IEnumerable<PostModel> models)
    {
        return models.Select(m => m.ToResponse()).ToList();
    }
}
