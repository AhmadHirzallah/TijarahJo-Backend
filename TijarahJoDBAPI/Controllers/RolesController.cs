using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Models;
using TijarahJoDB.BLL;
using TijarahJoDBAPI.DTOs.Requests;
using TijarahJoDBAPI.DTOs.Responses;
using TijarahJoDBAPI.DTOs.Mappers;
using TijarahJoDBAPI.Extensions;

namespace TijarahJoDBAPI.Controllers
{
    [ApiController]
    [Route("api/roles")]
    [Produces("application/json")]
    public class RolesController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Retrieves all roles
        /// </summary>
        /// <returns>A list of all user roles</returns>
        [HttpGet]
        [EndpointSummary("Retrieves all roles")]
        [EndpointDescription("Returns a list of all available user roles in the system.")]
        [EndpointName("GetAllRoles")]
        [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<RoleResponse>> GetAll()
        {
            var rolesList = Role.GetAllTbRoles();

            if (rolesList == null || rolesList.Rows.Count == 0)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "No Roles Found",
                    Detail = "There are no roles defined in the system.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var responseList = new List<RoleResponse>();

            foreach (System.Data.DataRow row in rolesList.Rows)
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

        /// <summary>
        /// Retrieves a role by ID
        /// </summary>
        /// <param name="id">The role ID</param>
        /// <returns>The role details</returns>
        [HttpGet("{id:int}", Name = "GetRoleById")]
        [EndpointSummary("Retrieves a role by ID")]
        [EndpointDescription("Returns detailed information about a specific user role.")]
        [EndpointName("GetRoleById")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult<RoleResponse> GetById(int id)
        {
            if (id < 1)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid ID",
                    Detail = $"The provided ID '{id}' is not valid. ID must be a positive integer.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            Role role = Role.Find(id);

            if (role == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Role Not Found",
                    Detail = $"No role found with ID '{id}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(role.RoleModel.ToResponse());
        }

        /// <summary>
        /// Checks if a role exists
        /// </summary>
        /// <param name="id">The role ID to check</param>
        /// <returns>Boolean indicating existence</returns>
        [HttpGet("{id:int}/exists")]
        [EndpointSummary("Checks if a role exists")]
        [EndpointDescription("Returns true if a role with the specified ID exists, false otherwise.")]
        [EndpointName("CheckRoleExists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public ActionResult<bool> Exists(int id)
        {
            if (id < 1)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid ID",
                    Detail = $"The provided ID '{id}' is not valid. ID must be a positive integer.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return Ok(Role.DoesRoleExist(id));
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Creates a new role (Admin only)
        /// </summary>
        /// <param name="request">The role data</param>
        /// <returns>The created role</returns>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [EndpointSummary("Creates a new role (Admin only)")]
        [EndpointDescription("Adds a new user role to the system. Requires Admin privileges.")]
        [EndpointName("CreateRole")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public ActionResult<RoleResponse> Create([FromBody] CreateRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Role role = new Role(new RoleModel
            (
                null,
                request.RoleName,
                DateTime.UtcNow,
                false
            ));

            if (!role.Save())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Creation Failed",
                    Detail = "An error occurred while creating the role.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }

            var response = role.RoleModel.ToResponse();

            return CreatedAtRoute("GetRoleById", new { id = response.RoleID }, response);
        }

        #endregion

        #region PUT Endpoints

        /// <summary>
        /// Updates an existing role (Admin only)
        /// </summary>
        /// <param name="id">The role ID</param>
        /// <param name="request">The updated role data</param>
        /// <returns>The updated role</returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [EndpointSummary("Updates an existing role (Admin only)")]
        [EndpointDescription("Updates the details of an existing user role. Requires Admin privileges.")]
        [EndpointName("UpdateRole")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public ActionResult<RoleResponse> Update(int id, [FromBody] UpdateRoleRequest request)
        {
            if (id < 1)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid ID",
                    Detail = $"The provided ID '{id}' is not valid. ID must be a positive integer.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Role role = Role.Find(id);

            if (role == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Role Not Found",
                    Detail = $"No role found with ID '{id}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            role.RoleName = request.RoleName;
            role.IsDeleted = request.IsDeleted;

            if (!role.Save())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Update Failed",
                    Detail = "An error occurred while updating the role.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }

            return Ok(role.RoleModel.ToResponse());
        }

        #endregion

        #region DELETE Endpoints

        /// <summary>
        /// Deletes a role (Admin only)
        /// </summary>
        /// <param name="id">The role ID to delete</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [EndpointSummary("Deletes a role (Admin only)")]
        [EndpointDescription("Soft deletes a user role from the system. Requires Admin privileges.")]
        [EndpointName("DeleteRole")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            if (id < 1)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid ID",
                    Detail = $"The provided ID '{id}' is not valid. ID must be a positive integer.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            // Prevent deleting core roles (Admin, User, Moderator)
            if (id <= 3)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Operation Not Allowed",
                    Detail = "Cannot delete core system roles (Admin, User, Moderator).",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (!Role.DeleteRole(id))
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Role Not Found",
                    Detail = $"No role found with ID '{id}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }

        #endregion
    }
}
