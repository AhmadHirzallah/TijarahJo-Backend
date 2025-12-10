using Microsoft.AspNetCore.Mvc;
using Models;
using System.Collections.Generic;
using TijarahJoDB.BLL;
using TijarahJoDB.DAL;
using TijarahJoDBAPI.DTOs.Requests;

namespace TijarahJoDBAPI.Controllers
{

	[ApiController]
	[Route("api/TbPosts")]
	public class UserPostsController : ControllerBase
	{

		[HttpGet("All", Name = "GetAllTbUserPosts")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<IEnumerable<PostModel>> GetAllTbUserPosts()
		{
			var tbuserpostsList = Post.GetAllTbUserPosts();

			if (tbuserpostsList == null || tbuserpostsList.Rows.Count == 0)
			{
				return NotFound("No TbPosts Found!");
			}

			var dtoList = new List<PostModel>();

			foreach (System.Data.DataRow row in tbuserpostsList.Rows)
			{
				dtoList.Add(new PostModel
				(
					(int?)row["PostID"],
					(int)row["UserID"],
					(int)row["CategoryID"],
					(string)row["PostTitle"],
					(string)row["PostDescription"],
					(decimal?)row["Price"],
					(int)row["Status"],
					(DateTime)row["CreatedAt"],
					(bool)row["IsDeleted"]
				));
			}

			return Ok(dtoList);
		}

		[HttpGet("pagination", Name = "GetPostsPaginated")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<IEnumerable<PostModel>> GetPaginated([FromQuery] GetPaginatedRequest getPaginatedRequest)
		{
			var tbuserpostsList = Post.GetPaginated(
				getPaginatedRequest.PageNumber,
				getPaginatedRequest.RowsPerPage,
				getPaginatedRequest.IncludeDeleted);

			if (tbuserpostsList == null || tbuserpostsList.Rows.Count == 0)
			{
				return NotFound("No TbPosts Found!");
			}

			var dtoList = new List<PostModel>();

			foreach (System.Data.DataRow row in tbuserpostsList.Rows)
			{
				dtoList.Add(new PostModel
				(
					(int?)row["PostID"],
					(int)row["UserID"],
					(int)row["CategoryID"],
					(string)row["PostTitle"],
					(string)row["PostDescription"],
					(decimal?)row["Price"],
					(int)row["Status"],
					(DateTime)row["CreatedAt"],
					(bool)row["IsDeleted"]
				));
			}

			return Ok(dtoList);
		}


		[HttpGet("{id}", Name = "GetPostById")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<PostModel> GetPostById(int id)
		{
			if (id < 1)
			{
				return BadRequest($"Not accepted ID {id}");
			}

			Post post = Post.Find(id);

			if (post == null)
			{
				return NotFound($"Post with ID {id} not found.");
			}

			PostModel dto = post.PostModel;

			return Ok(dto);
		}

		[HttpPost(Name = "AddPost")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ActionResult<PostModel> AddPost(PostModel newPostDTO)
		{
			if (newPostDTO == null || newPostDTO.UserID < 0 || newPostDTO.CategoryID < 0 || string.IsNullOrEmpty(newPostDTO.PostTitle) || newPostDTO.Status < 0)
			{
				return BadRequest("Invalid Post data.");
			}

			Post post = new Post(new PostModel
			(
					newPostDTO.PostID,
					newPostDTO.UserID,
					newPostDTO.CategoryID,
					newPostDTO.PostTitle,
					newPostDTO.PostDescription,
					newPostDTO.Price,
					newPostDTO.Status,
					newPostDTO.CreatedAt,
					newPostDTO.IsDeleted
			));

			if (!post.Save())
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Error adding Post");
			}

			newPostDTO.PostID = post.PostID;

			return CreatedAtRoute("GetPostById", new { id = newPostDTO.PostID }, newPostDTO);
		}

		[HttpPut("{id}", Name = "UpdatePost")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<PostModel> UpdatePost(int id, PostModel updatedPost)
		{
			if (id < 1 || updatedPost == null || updatedPost.UserID < 0 || updatedPost.CategoryID < 0 || string.IsNullOrEmpty(updatedPost.PostTitle) || updatedPost.Status < 0)
			{
				return BadRequest("Invalid Post data.");
			}

			Post post = Post.Find(id);

			if (post == null)
			{
				return NotFound($"Post with ID {id} not found.");
			}

			post.UserID = updatedPost.UserID;
			post.CategoryID = updatedPost.CategoryID;
			post.PostTitle = updatedPost.PostTitle;
			post.PostDescription = updatedPost.PostDescription;
			post.Price = updatedPost.Price;
			post.Status = updatedPost.Status;
			post.CreatedAt = updatedPost.CreatedAt;
			post.IsDeleted = updatedPost.IsDeleted;

			if (!post.Save())
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Error updating Post");
			}

			return Ok(post.PostModel);
		}

		[HttpDelete("{id}", Name = "DeletePost")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult DeletePost(int id)
		{
			if (id < 1)
			{
				return BadRequest($"Not accepted ID {id}");
			}

			if (Post.DeletePost(id))
			{
				return Ok($"Post with ID {id} has been deleted.");
			}
			else
			{
				return NotFound($"Post with ID {id} not found. No rows deleted!");
			}
		}

		[HttpGet("Exists/{id}", Name = "DoesPostExist")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ActionResult<bool> DoesPostExist(int id)
		{
			if (id < 1)
			{
				return BadRequest($"Not accepted ID {id}");
			}

			bool exists = Post.DoesPostExist(id);

			return Ok(exists);
		}
	}
}
