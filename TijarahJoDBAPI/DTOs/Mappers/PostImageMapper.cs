using Models;
using TijarahJoDBAPI.DTOs.Responses;

namespace TijarahJoDBAPI.DTOs.Mappers;

public static class PostImageMapper
{
    public static PostImageResponse ToResponse(this PostImageModel model)
    {
        return new PostImageResponse
        {
            PostImageID = model.PostImageID,
            PostID = model.PostID,
            PostImageURL = model.PostImageURL,
            UploadedAt = model.UploadedAt,
            IsDeleted = model.IsDeleted
        };
    }

    public static List<PostImageResponse> ToResponseList(this IEnumerable<PostImageModel> models)
    {
        return models.Select(m => m.ToResponse()).ToList();
    }
}
