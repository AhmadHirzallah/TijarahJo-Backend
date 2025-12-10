using Microsoft.AspNetCore.Mvc;
using Models;
using System.Collections.Generic;
using TijarahJoDB.BLL;
using TijarahJoDB.DAL;

namespace TijarahJoDBAPI.Controllers
{
	[ApiController]
	[Route("api/TbPostImages")]
	public class PostImagesController : ControllerBase
	{

		[HttpGet("All", Name = "GetAllTbPostImages")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<IEnumerable<PostImageModel>> GetAllTbPostImages()
		{
			var tbpostimagesList = PostImage.GetAllTbPostImages();

			if (tbpostimagesList == null || tbpostimagesList.Rows.Count == 0)
			{
				return NotFound("No TbPostImages Found!");
			}

			var dtoList = new List<PostImageModel>();

			foreach (System.Data.DataRow row in tbpostimagesList.Rows)
			{
				dtoList.Add(new PostImageModel
				(
					(int?)row["PostImageID"],
					(int)row["PostID"],
					(string)row["PostImageURL"],
					(DateTime)row["UploadedAt"],
					(bool)row["IsDeleted"]
				));
			}

			return Ok(dtoList);
		}

		[HttpGet("{id}", Name = "GetPostImageById")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<PostImageModel> GetPostImageById(int id)
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

			PostImageModel dto = postimage.PostImageModel;

			return Ok(dto);
		}

		[HttpPost(Name = "AddPostImage")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ActionResult<PostImageModel> AddPostImage(PostImageModel newPostImageDTO)
		{
			if (newPostImageDTO == null || newPostImageDTO.PostID < 0 || string.IsNullOrEmpty(newPostImageDTO.PostImageURL))
			{
				return BadRequest("Invalid PostImage data.");
			}

			PostImage postimage = new PostImage(new PostImageModel
			(
					newPostImageDTO.PostImageID,
					newPostImageDTO.PostID,
					newPostImageDTO.PostImageURL,
					newPostImageDTO.UploadedAt,
					newPostImageDTO.IsDeleted
			));

			if (!postimage.Save())
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Error adding PostImage");
			}

			newPostImageDTO.PostImageID = postimage.PostImageID;

			return CreatedAtRoute("GetPostImageById", new { id = newPostImageDTO.PostImageID }, newPostImageDTO);
		}

		[HttpPut("{id}", Name = "UpdatePostImage")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<PostImageModel> UpdatePostImage(int id, PostImageModel updatedPostImage)
		{
			if (id < 1 || updatedPostImage == null || updatedPostImage.PostID < 0 || string.IsNullOrEmpty(updatedPostImage.PostImageURL))
			{
				return BadRequest("Invalid PostImage data.");
			}

			PostImage postimage = PostImage.Find(id);

			if (postimage == null)
			{
				return NotFound($"PostImage with ID {id} not found.");
			}

			postimage.PostID = updatedPostImage.PostID;
			postimage.PostImageURL = updatedPostImage.PostImageURL;
			postimage.UploadedAt = updatedPostImage.UploadedAt;
			postimage.IsDeleted = updatedPostImage.IsDeleted;

			if (!postimage.Save())
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Error updating PostImage");
			}

			return Ok(postimage.PostImageModel);
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
