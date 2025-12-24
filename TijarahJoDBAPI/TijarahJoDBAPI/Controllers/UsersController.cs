using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TijarahJoDB.BLL;
using Models;
using Microsoft.AspNetCore.Authorization;
using TijarahJoDBAPI.DTOs.Requests;
using TijarahJoDBAPI.DTOs.Responses;
using TijarahJoDBAPI.DTOs.Mappers;

namespace TijarahJoDBAPI.Controllers
{
	[ApiController]
	[Route("api/TbUsers")]
	public class UsersController(TokenService tokenService) : ControllerBase
	{

        [HttpGet("All", Name = "GetAllTbUsers")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<IEnumerable<UserResponse>> GetAllTbUsers()
		{
			var tbusersList = UserBL.GetAllTbUsers();

			if (tbusersList == null || tbusersList.Rows.Count == 0)
			{
				return NotFound("No TbUsers Found!");
			}

			var responseList = new List<UserResponse>();

			foreach (System.Data.DataRow row in tbusersList.Rows)
			{
				responseList.Add(new UserResponse
				{
					UserID = (int?)row["UserID"],
					Username = (string)row["Username"],
					Email = (string)row["Email"],
					FirstName = (string)row["FirstName"],
					LastName = row["LastName"] == DBNull.Value ? null : (string)row["LastName"],
					JoinDate = (DateTime)row["JoinDate"],
					Status = (int)row["Status"],
					RoleID = (int)row["RoleID"],
					IsDeleted = (bool)row["IsDeleted"]
				});
			}

			return Ok(responseList);
		}

		[HttpGet("{id}", Name = "GetUserById")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<UserResponse> GetUserById(int id)
		{
			if (id < 1)
			{
				return BadRequest($"Not accepted ID {id}");
			}

			UserBL user = UserBL.Find(id);

			if (user == null)
			{
				return NotFound($"User with ID {id} not found.");
			}

			return Ok(user.UserModel.ToResponse());
        }



		[HttpPost(Name = "Register")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ActionResult<UserResponse> Register([FromBody] CreateUserRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			UserBL user = new UserBL(new UserModel
			(
				null, // UserID - will be generated
				request.Username,
				request.Password, // Note: Should be hashed before storing
				request.Email,
				request.FirstName,
				request.LastName,
				DateTime.UtcNow, // JoinDate
				request.Status,
				request.RoleID,
				false // IsDeleted
			));

			if (!user.Save())
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Error adding User");
			}

			var response = user.UserModel.ToResponse();

			return CreatedAtRoute("GetUserById", new { id = response.UserID }, response);
		}


        // To-do
		//[Authorize]

		[HttpPut("{id}", Name = "UpdateUser")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<UserResponse> UpdateUser(int id, [FromBody] UpdateUserRequest request)
		{
			if (id < 1)
			{
				return BadRequest($"Not accepted ID {id}");
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			UserBL user = UserBL.Find(id);

			if (user == null)
			{
				return NotFound($"User with ID {id} not found.");
			}

			user.Username = request.Username;
			user.HashedPassword = request.Password; // Note: Should be hashed
			user.Email = request.Email;
			user.FirstName = request.FirstName;
			user.LastName = request.LastName;
			user.Status = request.Status;
			user.RoleID = request.RoleID;
			user.IsDeleted = request.IsDeleted;

			if (!user.Save())
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Error updating User");
			}

			return Ok(user.UserModel.ToResponse());
		}



			// To-do
        [HttpPost("login", Name = "Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            UserBL user = UserBL.Login(request.Login, request.Password);

            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            if (user.UserID == null)
            {
                return Unauthorized("User not found.");
            }

            if (user.IsDeleted)
            {
                return Unauthorized("User is deleted.");
            }

            string token = tokenService.CreateAuthToken(user.UserID);

            var response = new LoginResponse
            {
                User = user.UserModel.ToResponse(),
                Token = token
            };

            return Ok(response);
        }
		/*
		*/





		//[Authorize]
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
				return Ok($"User with ID {id} has been deleted.");
			}
			else
			{
				return NotFound($"User with ID {id} not found. No rows deleted!");
			}
		}

		[HttpGet("Exists/{id}", Name = "DoesUserExist")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[Authorize]
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
