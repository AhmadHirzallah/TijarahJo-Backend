using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TijarahJoDB.BLL;
using Models;
using Microsoft.AspNetCore.Authorization;

namespace TijarahJoDBAPI.Controllers
{
	[ApiController]
	[Route("api/TbUsers")]
    // To-do
	//public class UsersController(/*JwtOptions jwtOptions,*/ TokenService tokenService) : ControllerBase
	public class UsersController() : ControllerBase
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

			return Ok(dto); // In axios -> response.data .. so u can do: ....Username
        }



		[HttpPost(Name = "Register")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ActionResult<UserModel> Register(UserModel newUserModel)
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

			// To-do
			//string Token = tokenService.CreateAuthToken(user.UserID);

            /// Axios in Frontend ... You will receive object and AXIOS reponse.data.User and response.data.Token
			/// reponse.data.User.UserName , Password, 
			/// Axios always with .data
			// To-do
            //return CreatedAtRoute("GetUserById", new { id = newUserModel.UserID }, new { User = user.UserModel, Token });
            return CreatedAtRoute("GetUserById", new { id = newUserModel.UserID });
		}


        // To-do
		//[Authorize]

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


		/*/

			// To-do
        [HttpPost("login", Name = "Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<UserModel> Login([FromBody] LoginRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Login) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Invalid login data.");
            }

            UserBL user = UserBL.Login(request.Login, request.Password);

            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

			/// Optional: block deleted users (if your SP already checks IsDeleted = 0, this is extra safety)
			/// if (user.IsDeleted)
			///	{
			///    return Unauthorized("User is deleted.");
			/// }


			if (user.UserID == null)
			{
				return Unauthorized("User not found.");
			}

			if (user.IsDeleted)
			{
				return Unauthorized("User is deleted.");
			}

			string Token = tokenService.CreateAuthToken(user.UserID);
            /// Axios in Frontend ... You will receive object and AXIOS reponse.data.User and response.data.Token
			/// reponse.data.User.UserName , Password, 
			/// Axios always with .data
            return Ok(new { User = user.UserModel, Token });
        }
		*/





        // To-do
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
