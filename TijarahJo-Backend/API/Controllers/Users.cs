using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TijarahJoDB.BLL;
using TijarahJoDB.DAL;

namespace TijarahJoDBAPI.Controllers
{
    [ApiController]
    [Route("api/TbUsers")]
    public class TbUsersController : ControllerBase
    {

        [HttpGet("All", Name = "GetAllTbUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<UserDTO>> GetAllTbUsers()
        {
            var tbusersList = User.GetAllTbUsers();
            
            if (tbusersList == null || tbusersList.Rows.Count == 0)
            {
                return NotFound("No TbUsers Found!");
            }

            var dtoList = new List<UserDTO>();
            
            foreach (System.Data.DataRow row in tbusersList.Rows)
            {
                dtoList.Add(new UserDTO
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
        public ActionResult<UserDTO> GetUserById(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            User user = User.Find(id);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            UserDTO dto = user.UserDTO;

            return Ok(dto);
        }

        [HttpPost(Name = "AddUser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserDTO> AddUser(UserDTO newUserDTO)
        {
            if (newUserDTO == null || string.IsNullOrEmpty(newUserDTO.Username) || string.IsNullOrEmpty(newUserDTO.HashedPassword) || string.IsNullOrEmpty(newUserDTO.Email) || string.IsNullOrEmpty(newUserDTO.FirstName) || newUserDTO.Status < 0 || newUserDTO.RoleID < 0)
            {
                return BadRequest("Invalid User data.");
            }

            User user = new User(new UserDTO
            (
                    newUserDTO.UserID,
                    newUserDTO.Username,
                    newUserDTO.HashedPassword,
                    newUserDTO.Email,
                    newUserDTO.FirstName,
                    newUserDTO.LastName,
                    newUserDTO.JoinDate,
                    newUserDTO.Status,
                    newUserDTO.RoleID,
                    newUserDTO.IsDeleted
            ));
            
            if (!user.Save())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding User");
            }

            newUserDTO.UserID = user.UserID;

            return CreatedAtRoute("GetUserById", new { id = newUserDTO.UserID }, newUserDTO);
        }

        [HttpPut("{id}", Name = "UpdateUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<UserDTO> UpdateUser(int id, UserDTO updatedUser)
        {
            if (id < 1 || updatedUser == null || string.IsNullOrEmpty(updatedUser.Username) || string.IsNullOrEmpty(updatedUser.HashedPassword) || string.IsNullOrEmpty(updatedUser.Email) || string.IsNullOrEmpty(updatedUser.FirstName) || updatedUser.Status < 0 || updatedUser.RoleID < 0)
            {
                return BadRequest("Invalid User data.");
            }

            User user = User.Find(id);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
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
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating User");
            }

            return Ok(user.UserDTO);
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

            if (User.DeleteUser(id))
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
        public ActionResult<bool> DoesUserExist(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            bool exists = User.DoesUserExist(id);

            return Ok(exists);
        }
    }
}
