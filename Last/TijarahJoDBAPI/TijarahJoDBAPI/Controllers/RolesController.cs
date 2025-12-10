using Microsoft.AspNetCore.Mvc;
using Models;
using System.Collections.Generic;
using TijarahJoDB.BLL;

namespace TijarahJoDBAPI.Controllers
{
	[ApiController]
	[Route("api/TbRoles")]
	public class RolesController : ControllerBase
	{

		[HttpGet("All", Name = "GetAllTbRoles")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<IEnumerable<RoleModel>> GetAllTbRoles()
		{
			var tbrolesList = Role.GetAllTbRoles();

			if (tbrolesList == null || tbrolesList.Rows.Count == 0)
			{
				return NotFound("No TbRoles Found!");
			}

			var dtoList = new List<RoleModel>();

			foreach (System.Data.DataRow row in tbrolesList.Rows)
			{
				dtoList.Add(new RoleModel
				(
					(int?)row["RoleID"],
					(string)row["RoleName"],
					(DateTime)row["CreatedAt"],
					(bool)row["IsDeleted"]
				));
			}

			return Ok(dtoList);
		}

		[HttpGet("{id}", Name = "GetRoleById")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<RoleModel> GetRoleById(int id)
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

			RoleModel dto = role.RoleModel;

			return Ok(dto);
		}

		[HttpPost(Name = "AddRole")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ActionResult<RoleModel> AddRole(RoleModel newRoleModel)
		{
			if (newRoleModel == null || string.IsNullOrEmpty(newRoleModel.RoleName))
			{
				return BadRequest("Invalid Role data.");
			}

			Role role = new Role(new RoleModel
			(
					newRoleModel.RoleID,
					newRoleModel.RoleName,
					newRoleModel.CreatedAt,
					newRoleModel.IsDeleted
			));

			if (!role.Save())
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Error adding Role");
			}

			newRoleModel.RoleID = role.RoleID;

			return CreatedAtRoute("GetRoleById", new { id = newRoleModel.RoleID }, newRoleModel);
		}

		[HttpPut("{id}", Name = "UpdateRole")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<RoleModel> UpdateRole(int id, RoleModel updatedRole)
		{
			if (id < 1 || updatedRole == null || string.IsNullOrEmpty(updatedRole.RoleName))
			{
				return BadRequest("Invalid Role data.");
			}

			Role role = Role.Find(id);

			if (role == null)
			{
				return NotFound($"Role with ID {id} not found.");
			}

			role.RoleName = updatedRole.RoleName;
			role.CreatedAt = updatedRole.CreatedAt;
			role.IsDeleted = updatedRole.IsDeleted;

			if (!role.Save())
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Error updating Role");
			}

			return Ok(role.RoleModel);
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
