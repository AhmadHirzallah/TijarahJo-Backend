using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TijarahJoDB.BLL;
using Models;

namespace TijarahJoDBAPI.Controllers
{
	[ApiController]
	[Route("api/TbUsers")]
	public class UsersController : ControllerBase
	{

		[HttpGet("All", Name = "GetAllTbUsers")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<IEnumerable<UserModel>> GetAllTbUsers()
		{
			var tbusersList = UserBL.GetAllTbUsers();

			if (tbusersList == null || tbusersList.Rows.Count == 0)
			{
				return NotFound("No TbUsers Found!");
			}

			var dtoList = new List<UserModel>();

			foreach (System.Data.DataRow row in tbusersList.Rows)
			{
				dtoList.Add(new UserModel
				(
					(int?)row["UserID"],
					(string)row["Username"],
					(string)row["HashedPassword"],
					(string)row["Email"],
					(string)row["FirstName"],
					(string)row["LastName"],
					(DateTime)row["JoinDate"],
					(int)row["Status"],
					(int)row["RoleID"],
					(bool)row["IsDeleted"]
				));
			}

			return Ok(dtoList);
		}

		[HttpGet("{id}", Name = "GetUserById")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<UserModel> GetUserById(int id)
		{
			if (id < 1)
			{
				return BadRequest($"Not accepted ID {id}");
			}

			UserBL user = UserBL.Find(id);

			if (user == null)
			{
				return NotFound($"UserBL with ID {id} not found.");
			}

			UserModel dto = user.UserModel;

			return Ok(dto);
		}

		[HttpPost(Name = "AddUser")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ActionResult<UserModel> AddUser(UserModel newUserModel)
		{
			if (newUserModel == null || string.IsNullOrEmpty(newUserModel.Username) || string.IsNullOrEmpty(newUserModel.HashedPassword) || string.IsNullOrEmpty(newUserModel.Email) || string.IsNullOrEmpty(newUserModel.FirstName) || newUserModel.Status < 0 || newUserModel.RoleID < 0)
			{
				return BadRequest("Invalid UserBL data.");
			}

			UserBL user = new UserBL(new UserModel
			(
					newUserModel.UserID,
					newUserModel.Username,
					newUserModel.HashedPassword,
					newUserModel.Email,
					newUserModel.FirstName,
					newUserModel.LastName,
					newUserModel.JoinDate,
					newUserModel.Status,
					newUserModel.RoleID,
					newUserModel.IsDeleted
			));

			if (!user.Save())
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Error adding UserBL");
			}

			newUserModel.UserID = user.UserID;

			return CreatedAtRoute("GetUserById", new { id = newUserModel.UserID }, newUserModel);
		}

		[HttpPut("{id}", Name = "UpdateUser")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<UserModel> UpdateUser(int id, UserModel updatedUser)
		{
			if (id < 1 || updatedUser == null || string.IsNullOrEmpty(updatedUser.Username) || string.IsNullOrEmpty(updatedUser.HashedPassword) || string.IsNullOrEmpty(updatedUser.Email) || string.IsNullOrEmpty(updatedUser.FirstName) || updatedUser.Status < 0 || updatedUser.RoleID < 0)
			{
				return BadRequest("Invalid UserBL data.");
			}

			UserBL user = UserBL.Find(id);

			if (user == null)
			{
				return NotFound($"UserBL with ID {id} not found.");
			}

			user.Username = updatedUser.Username;
			user.HashedPassword = updatedUser.HashedPassword;
			user.Email = updatedUser.Email;
			user.FirstName = updatedUser.FirstName;
			user.LastName = updatedUser.LastName;
			user.JoinDate = updatedUser.JoinDate;
			user.Status = updatedUser.Status;
			user.RoleID = updatedUser.RoleID;
			user.IsDeleted = updatedUser.IsDeleted;

			if (!user.Save())
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Error updating UserBL");
			}

			return Ok(user.UserModel);
		}

		[HttpDelete("{id}", Name = "DeleteUser")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult DeleteUser(int id)
		{
			if (id < 1)
			{
				return BadRequest($"Not accepted ID {id}");
			}

			if (UserBL.DeleteUser(id))
			{
				return Ok($"UserBL with ID {id} has been deleted.");
			}
			else
			{
				return NotFound($"UserBL with ID {id} not found. No rows deleted!");
			}
		}

		[HttpGet("Exists/{id}", Name = "DoesUserExist")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ActionResult<bool> DoesUserExist(int id)
		{
			if (id < 1)
			{
				return BadRequest($"Not accepted ID {id}");
			}

			bool exists = UserBL.DoesUserExist(id);

			return Ok(exists);
		}
	}
}
