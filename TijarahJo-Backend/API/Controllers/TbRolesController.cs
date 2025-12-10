//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;

//namespace API.Controllers
//{
//    [ApiController]
//    [Route("api/TbRoles")]
//    public class TbRolesController : ControllerBase
//    {

//        [HttpGet("All", Name = "GetAllTbRoles")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        public ActionResult<IEnumerable<Role>> GetAllTbRoles()
//        {
//            var tbrolesList = Role.GetAllTbRoles();
            
//            if (tbrolesList == null || tbrolesList.Rows.Count == 0)
//            {
//                return NotFound("No TbRoles Found!");
//            }

//            var dtoList = new List<RoleDTO>();
            
//            foreach (System.Data.DataRow row in tbrolesList.Rows)
//            {
//                dtoList.Add(new RoleDTO
//                (
//                    (int?)row["RoleID"],
//                    (string)row["RoleName"],
//                    (DateTime)row["CreatedAt"],
//                    (bool)row["IsDeleted"]
//                ));
//            }
            
//            return Ok(dtoList);
//        }

//        [HttpGet("{id}", Name = "GetRoleById")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        public ActionResult<RoleDTO> GetRoleById(int id)
//        {
//            if (id < 1)
//            {
//                return BadRequest($"Not accepted ID {id}");
//            }

//            Role role = Role.Find(id);

//            if (role == null)
//            {
//                return NotFound($"Role with ID {id} not found.");
//            }

//            RoleDTO dto = role.RoleDTO;

//            return Ok(dto);
//        }

//        [HttpPost(Name = "AddRole")]
//        [ProducesResponseType(StatusCodes.Status201Created)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        public ActionResult<RoleDTO> AddRole(RoleDTO newRoleDTO)
//        {
//            if (newRoleDTO == null || string.IsNullOrEmpty(newRoleDTO.RoleName))
//            {
//                return BadRequest("Invalid Role data.");
//            }

//            Role role = new Role(new RoleDTO
//            (
//                    newRoleDTO.RoleID,
//                    newRoleDTO.RoleName,
//                    newRoleDTO.CreatedAt,
//                    newRoleDTO.IsDeleted
//            ));
            
//            if (!role.Save())
//            {
//                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding Role");
//            }

//            newRoleDTO.RoleID = role.RoleID;

//            return CreatedAtRoute("GetRoleById", new { id = newRoleDTO.RoleID }, newRoleDTO);
//        }

//        [HttpPut("{id}", Name = "UpdateRole")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        public ActionResult<RoleDTO> UpdateRole(int id, RoleDTO updatedRole)
//        {
//            if (id < 1 || updatedRole == null || string.IsNullOrEmpty(updatedRole.RoleName))
//            {
//                return BadRequest("Invalid Role data.");
//            }

//            Role role = Role.Find(id);

//            if (role == null)
//            {
//                return NotFound($"Role with ID {id} not found.");
//            }

//            role.RoleName = updatedRole.RoleName;
//            role.CreatedAt = updatedRole.CreatedAt;
//            role.IsDeleted = updatedRole.IsDeleted;

//            if (!role.Save())
//            {
//                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating Role");
//            }

//            return Ok(role.RoleDTO);
//        }

//        [HttpDelete("{id}", Name = "DeleteRole")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        public ActionResult DeleteRole(int id)
//        {
//            if (id < 1)
//            {
//                return BadRequest($"Not accepted ID {id}");
//            }

//            if (Role.DeleteRole(id))
//            {
//                return Ok($"Role with ID {id} has been deleted.");
//            }
//            else
//            {
//                return NotFound($"Role with ID {id} not found. No rows deleted!");
//            }
//        }

//        [HttpGet("Exists/{id}", Name = "DoesRoleExist")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        public ActionResult<bool> DoesRoleExist(int id)
//        {
//            if (id < 1)
//            {
//                return BadRequest($"Not accepted ID {id}");
//            }

//            bool exists = Role.DoesRoleExist(id);

//            return Ok(exists);
//        }
//    }
//}
