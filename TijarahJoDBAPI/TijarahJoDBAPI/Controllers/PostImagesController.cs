using Microsoft.AspNetCore.Mvc;
using Models;
using TijarahJoDB.BLL;
using TijarahJoDBAPI.DTOs.Requests;
using TijarahJoDBAPI.DTOs.Responses;
using TijarahJoDBAPI.DTOs.Mappers;

namespace TijarahJoDBAPI.Controllers
{
    [ApiController]
    [Route("api/TbPostImages")]
    public class PostImagesController : ControllerBase
    {
        [HttpGet("All", Name = "GetAllTbPostImages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<PostImageResponse>> GetAllTbPostImages()
        {
            var tbpostimagesList = PostImage.GetAllTbPostImages();

            if (tbpostimagesList == null || tbpostimagesList.Rows.Count == 0)
            {
                return NotFound("No TbPostImages Found!");
            }

            var responseList = new List<PostImageResponse>();

            foreach (System.Data.DataRow row in tbpostimagesList.Rows)
            {
                responseList.Add(new PostImageResponse
                {
                    PostImageID = (int?)row["PostImageID"],
                    PostID = (int)row["PostID"],
                    PostImageURL = (string)row["PostImageURL"],
                    UploadedAt = (DateTime)row["UploadedAt"],
                    IsDeleted = (bool)row["IsDeleted"]
                });
            }

            return Ok(responseList);
        }

        [HttpGet("{id}", Name = "GetPostImageById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<PostImageResponse> GetPostImageById(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            PostImage postimage = PostImage.Find(id);

            if (postimage == null)
            {
                return NotFound($"PostImage with ID {id} not found.");
            }

            return Ok(postimage.PostImageModel.ToResponse());
        }

        [HttpPost(Name = "AddPostImage")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<PostImageResponse> AddPostImage([FromBody] CreatePostImageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PostImage postimage = new PostImage(new PostImageModel
            (
                null, // PostImageID - will be generated
                request.PostID,
                request.PostImageURL,
                DateTime.UtcNow, // UploadedAt
                false // IsDeleted
            ));

            if (!postimage.Save())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding PostImage");
            }

            var response = postimage.PostImageModel.ToResponse();

            return CreatedAtRoute("GetPostImageById", new { id = response.PostImageID }, response);
        }

        [HttpPut("{id}", Name = "UpdatePostImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<PostImageResponse> UpdatePostImage(int id, [FromBody] UpdatePostImageRequest request)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PostImage postimage = PostImage.Find(id);

            if (postimage == null)
            {
                return NotFound($"PostImage with ID {id} not found.");
            }

            postimage.PostID = request.PostID;
            postimage.PostImageURL = request.PostImageURL;
            postimage.IsDeleted = request.IsDeleted;

            if (!postimage.Save())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating PostImage");
            }

            return Ok(postimage.PostImageModel.ToResponse());
        }

        [HttpDelete("{id}", Name = "DeletePostImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeletePostImage(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            if (PostImage.DeletePostImage(id))
            {
                return Ok($"PostImage with ID {id} has been deleted.");
            }
            else
            {
                return NotFound($"PostImage with ID {id} not found. No rows deleted!");
            }
        }

        [HttpGet("Exists/{id}", Name = "DoesPostImageExist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<bool> DoesPostImageExist(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            bool exists = PostImage.DoesPostImageExist(id);

            return Ok(exists);
        }
    }
}
