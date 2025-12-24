using Microsoft.AspNetCore.Mvc;
using Models;
using TijarahJoDB.BLL;
using TijarahJoDBAPI.DTOs.Requests;
using TijarahJoDBAPI.DTOs.Responses;
using TijarahJoDBAPI.DTOs.Mappers;

namespace TijarahJoDBAPI.Controllers
{
    [ApiController]
    [Route("api/TbRoles")]
    public class RolesController : ControllerBase
    {
        [HttpGet("All", Name = "GetAllTbRoles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<RoleResponse>> GetAllTbRoles()
        {
            var tbrolesList = Role.GetAllTbRoles();

            if (tbrolesList == null || tbrolesList.Rows.Count == 0)
            {
                return NotFound("No TbRoles Found!");
            }

            var responseList = new List<RoleResponse>();

            foreach (System.Data.DataRow row in tbrolesList.Rows)
            {
                responseList.Add(new RoleResponse
                {
                    RoleID = (int?)row["RoleID"],
                    RoleName = (string)row["RoleName"],
                    CreatedAt = (DateTime)row["CreatedAt"],
                    IsDeleted = (bool)row["IsDeleted"]
                });
            }

            return Ok(responseList);
        }

        [HttpGet("{id}", Name = "GetRoleById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<RoleResponse> GetRoleById(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            Role role = Role.Find(id);

            if (role == null)
            {
                return NotFound($"Role with ID {id} not found.");
            }

            return Ok(role.RoleModel.ToResponse());
        }

        [HttpPost(Name = "AddRole")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<RoleResponse> AddRole([FromBody] CreateRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Role role = new Role(new RoleModel
            (
                null, // RoleID - will be generated
                request.RoleName,
                DateTime.UtcNow, // CreatedAt
                false // IsDeleted
            ));

            if (!role.Save())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding Role");
            }

            var response = role.RoleModel.ToResponse();

            return CreatedAtRoute("GetRoleById", new { id = response.RoleID }, response);
        }

        [HttpPut("{id}", Name = "UpdateRole")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<RoleResponse> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Role role = Role.Find(id);

            if (role == null)
            {
                return NotFound($"Role with ID {id} not found.");
            }

            role.RoleName = request.RoleName;
            role.IsDeleted = request.IsDeleted;

            if (!role.Save())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating Role");
            }

            return Ok(role.RoleModel.ToResponse());
        }

        [HttpDelete("{id}", Name = "DeleteRole")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteRole(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            if (Role.DeleteRole(id))
            {
                return Ok($"Role with ID {id} has been deleted.");
            }
            else
            {
                return NotFound($"Role with ID {id} not found. No rows deleted!");
            }
        }

        [HttpGet("Exists/{id}", Name = "DoesRoleExist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<bool> DoesRoleExist(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            bool exists = Role.DoesRoleExist(id);

            return Ok(exists);
        }
    }
}
