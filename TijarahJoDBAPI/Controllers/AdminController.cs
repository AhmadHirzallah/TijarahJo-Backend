using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using TijarahJoDB.BLL;
using TijarahJoDBAPI.DTOs.Requests;
using TijarahJoDBAPI.DTOs.Responses;
using TijarahJoDBAPI.DTOs.Mappers;
using TijarahJoDBAPI.Extensions;

namespace TijarahJoDBAPI.Controllers
{
    /// <summary>
    /// Admin-only controller for managing the entire TijarahJo platform.
    /// All endpoints require Admin role authentication.
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = RoleNames.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class AdminController : ControllerBase
    {
        #region Dashboard & Statistics

        /// <summary>
        /// Gets admin dashboard statistics
        /// </summary>
        /// <returns>Platform-wide statistics</returns>
        [HttpGet("dashboard")]
        [EndpointSummary("Gets admin dashboard statistics")]
        [EndpointDescription("Returns comprehensive platform statistics including user counts, post counts, and activity metrics.")]
        [EndpointName("GetAdminDashboard")]
        [ProducesResponseType(typeof(AdminDashboardResponse), StatusCodes.Status200OK)]
        public ActionResult<AdminDashboardResponse> GetDashboard()
        {
            var adminId = User.GetUserIdRequired();

            // Get statistics from various tables
            var usersTable = UserBL.GetAllTbUsers();
            var postsTable = Post.GetAllTbUserPosts();
            var categoriesTable = Category.GetAllTbItemCategories();
            var rolesTable = Role.GetAllTbRoles();

            var totalUsers = usersTable?.Rows.Count ?? 0;
            var activeUsers = usersTable?.Select("IsDeleted = 0").Length ?? 0;
            var deletedUsers = totalUsers - activeUsers;

            var totalPosts = postsTable?.Rows.Count ?? 0;
            var activePosts = postsTable?.Select("IsDeleted = 0").Length ?? 0;
            var deletedPosts = totalPosts - activePosts;

            var totalCategories = categoriesTable?.Rows.Count ?? 0;

            // Calculate posts by status
            var draftPosts = postsTable?.Select("Status = 0").Length ?? 0;
            var pendingPosts = postsTable?.Select("Status = 1").Length ?? 0;
            var publishedPosts = postsTable?.Select("Status = 2").Length ?? 0;

            return Ok(new AdminDashboardResponse
            {
                GeneratedAt = DateTime.UtcNow,
                GeneratedByAdminId = adminId,

                // User Statistics
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                DeletedUsers = deletedUsers,

                // Post Statistics
                TotalPosts = totalPosts,
                ActivePosts = activePosts,
                DeletedPosts = deletedPosts,
                DraftPosts = draftPosts,
                PendingReviewPosts = pendingPosts,
                PublishedPosts = publishedPosts,

                // Other Statistics
                TotalCategories = totalCategories,
                TotalRoles = rolesTable?.Rows.Count ?? 0
            });
        }

        #endregion

        #region User Management

        /// <summary>
        /// Gets all users with detailed information
        /// </summary>
        [HttpGet("users")]
        [EndpointSummary("Gets all users (Admin)")]
        [EndpointDescription("Returns a comprehensive list of all users with full details including deleted users.")]
        [EndpointName("AdminGetAllUsers")]
        [ProducesResponseType(typeof(AdminUsersListResponse), StatusCodes.Status200OK)]
        public ActionResult<AdminUsersListResponse> GetAllUsers([FromQuery] bool includeDeleted = true)
        {
            var usersTable = UserBL.GetAllTbUsers();

            if (usersTable == null || usersTable.Rows.Count == 0)
            {
                return Ok(new AdminUsersListResponse
                {
                    Users = new List<AdminUserResponse>(),
                    TotalCount = 0,
                    ActiveCount = 0,
                    DeletedCount = 0
                });
            }

            var users = new List<AdminUserResponse>();

            foreach (DataRow row in usersTable.Rows)
            {
                var isDeleted = (bool)row["IsDeleted"];
                if (!includeDeleted && isDeleted) continue;

                users.Add(new AdminUserResponse
                {
                    UserID = (int)row["UserID"],
                    Username = (string)row["Username"],
                    Email = (string)row["Email"],
                    FirstName = (string)row["FirstName"],
                    LastName = row["LastName"] == DBNull.Value ? null : (string)row["LastName"],
                    JoinDate = (DateTime)row["JoinDate"],
                    Status = (int)row["Status"],
                    RoleID = (int)row["RoleID"],
                    RoleName = GetRoleNameFromId((int)row["RoleID"]),
                    IsDeleted = isDeleted
                });
            }

            return Ok(new AdminUsersListResponse
            {
                Users = users,
                TotalCount = users.Count,
                ActiveCount = users.Count(u => !u.IsDeleted),
                DeletedCount = users.Count(u => u.IsDeleted)
            });
        }

        /// <summary>
        /// Gets a specific user by ID with full details
        /// </summary>
        [HttpGet("users/{id:int}")]
        [EndpointSummary("Gets user details (Admin)")]
        [EndpointDescription("Returns complete user information including activity statistics.")]
        [EndpointName("AdminGetUserById")]
        [ProducesResponseType(typeof(AdminUserDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult<AdminUserDetailResponse> GetUserById(int id)
        {
            if (id < 1)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid ID",
                    Detail = "User ID must be a positive integer.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var user = UserBL.Find(id);
            if (user == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = $"No user found with ID '{id}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            // Get user's posts count
            var allPosts = Post.GetAllTbUserPosts();
            var userPostsCount = allPosts?.Select($"UserID = {id}").Length ?? 0;

            // Get user's images count
            var userImages = UserImage.GetImagesByUserId(id);

            return Ok(new AdminUserDetailResponse
            {
                UserID = user.UserID ?? 0,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                JoinDate = user.JoinDate,
                Status = user.Status,
                RoleID = user.RoleID,
                RoleName = GetRoleNameFromId(user.RoleID),
                IsDeleted = user.IsDeleted,

                // Activity stats
                TotalPosts = userPostsCount,
                TotalImages = userImages?.Rows.Count ?? 0
            });
        }

        /// <summary>
        /// Updates a user's role
        /// </summary>
        [HttpPut("users/{id:int}/role")]
        [EndpointSummary("Updates user role (Admin)")]
        [EndpointDescription("Changes a user's role. Cannot demote yourself from Admin.")]
        [EndpointName("AdminUpdateUserRole")]
        [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult<AdminUserResponse> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
        {
            var currentAdminId = User.GetUserIdRequired();

            // Prevent admin from demoting themselves
            if (id == currentAdminId && request.RoleID != 1) // Assuming 1 = Admin
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Operation Not Allowed",
                    Detail = "You cannot demote yourself from Admin role.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var user = UserBL.Find(id);
            if (user == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = $"No user found with ID '{id}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            user.RoleID = request.RoleID;

            if (!user.Save())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Update Failed",
                    Detail = "Failed to update user role.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }

            return Ok(new AdminUserResponse
            {
                UserID = user.UserID ?? 0,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                JoinDate = user.JoinDate,
                Status = user.Status,
                RoleID = user.RoleID,
                RoleName = GetRoleNameFromId(user.RoleID),
                IsDeleted = user.IsDeleted
            });
        }

        /// <summary>
        /// Updates a user's status (ban/activate)
        /// </summary>
        [HttpPut("users/{id:int}/status")]
        [EndpointSummary("Updates user status (Admin)")]
        [EndpointDescription("Activates, suspends, or bans a user account.")]
        [EndpointName("AdminUpdateUserStatus")]
        [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult<AdminUserResponse> UpdateUserStatus(int id, [FromBody] UpdateUserStatusRequest request)
        {
            var currentAdminId = User.GetUserIdRequired();

            // Prevent admin from banning themselves
            if (id == currentAdminId)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Operation Not Allowed",
                    Detail = "You cannot change your own status.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var user = UserBL.Find(id);
            if (user == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = $"No user found with ID '{id}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            user.Status = request.Status;

            if (!user.Save())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Update Failed",
                    Detail = "Failed to update user status.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }

            return Ok(new AdminUserResponse
            {
                UserID = user.UserID ?? 0,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                JoinDate = user.JoinDate,
                Status = user.Status,
                RoleID = user.RoleID,
                RoleName = GetRoleNameFromId(user.RoleID),
                IsDeleted = user.IsDeleted
            });
        }

        /// <summary>
        /// Hard deletes a user (permanent)
        /// </summary>
        [HttpDelete("users/{id:int}/permanent")]
        [EndpointSummary("Permanently deletes a user (Admin)")]
        [EndpointDescription("Permanently removes a user from the system. This action cannot be undone.")]
        [EndpointName("AdminPermanentDeleteUser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public IActionResult PermanentDeleteUser(int id)
        {
            var currentAdminId = User.GetUserIdRequired();

            if (id == currentAdminId)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Operation Not Allowed",
                    Detail = "You cannot delete your own account.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (!UserBL.DeleteUser(id))
            {
                return NotFound(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = $"No user found with ID '{id}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }

        /// <summary>
        /// Restores a soft-deleted user
        /// </summary>
        [HttpPost("users/{id:int}/restore")]
        [EndpointSummary("Restores a deleted user (Admin)")]
        [EndpointDescription("Restores a soft-deleted user account.")]
        [EndpointName("AdminRestoreUser")]
        [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult<AdminUserResponse> RestoreUser(int id)
        {
            var user = UserBL.Find(id);
            if (user == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = $"No user found with ID '{id}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            user.IsDeleted = false;
            user.Status = 0; // Active

            if (!user.Save())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Restore Failed",
                    Detail = "Failed to restore user.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }

            return Ok(new AdminUserResponse
            {
                UserID = user.UserID ?? 0,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                JoinDate = user.JoinDate,
                Status = user.Status,
                RoleID = user.RoleID,
                RoleName = GetRoleNameFromId(user.RoleID),
                IsDeleted = user.IsDeleted
            });
        }

        #endregion

        #region Post Management

        /// <summary>
        /// Gets all posts with admin details
        /// </summary>
        [HttpGet("posts")]
        [EndpointSummary("Gets all posts (Admin)")]
        [EndpointDescription("Returns all posts with full details including deleted posts.")]
        [EndpointName("AdminGetAllPosts")]
        [ProducesResponseType(typeof(AdminPostsListResponse), StatusCodes.Status200OK)]
        public ActionResult<AdminPostsListResponse> GetAllPosts([FromQuery] bool includeDeleted = true)
        {
            var postsTable = Post.GetAllTbUserPosts();

            if (postsTable == null || postsTable.Rows.Count == 0)
            {
                return Ok(new AdminPostsListResponse
                {
                    Posts = new List<AdminPostResponse>(),
                    TotalCount = 0
                });
            }

            var posts = new List<AdminPostResponse>();

            foreach (DataRow row in postsTable.Rows)
            {
                var isDeleted = (bool)row["IsDeleted"];
                if (!includeDeleted && isDeleted) continue;

                posts.Add(new AdminPostResponse
                {
                    PostID = (int)row["PostID"],
                    UserID = (int)row["UserID"],
                    CategoryID = (int)row["CategoryID"],
                    PostTitle = (string)row["PostTitle"],
                    PostDescription = row["PostDescription"] == DBNull.Value ? null : (string)row["PostDescription"],
                    Price = row["Price"] == DBNull.Value ? null : (decimal?)row["Price"],
                    Status = (int)row["Status"],
                    StatusName = GetPostStatusName((int)row["Status"]),
                    CreatedAt = (DateTime)row["CreatedAt"],
                    IsDeleted = isDeleted
                });
            }

            return Ok(new AdminPostsListResponse
            {
                Posts = posts,
                TotalCount = posts.Count,
                ByStatus = new Dictionary<string, int>
                {
                    { "Draft", posts.Count(p => p.Status == 0) },
                    { "PendingReview", posts.Count(p => p.Status == 1) },
                    { "Active", posts.Count(p => p.Status == 2) },
                    { "Sold", posts.Count(p => p.Status == 3) },
                    { "Deleted", posts.Count(p => p.IsDeleted) }
                }
            });
        }

        /// <summary>
        /// Updates a post's status (approve/reject/remove)
        /// </summary>
        [HttpPut("posts/{id:int}/status")]
        [EndpointSummary("Updates post status (Admin)")]
        [EndpointDescription("Approves, rejects, or removes a post.")]
        [EndpointName("AdminUpdatePostStatus")]
        [ProducesResponseType(typeof(AdminPostResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult<AdminPostResponse> UpdatePostStatus(int id, [FromBody] UpdatePostStatusRequest request)
        {
            var post = Post.Find(id);
            if (post == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Post Not Found",
                    Detail = $"No post found with ID '{id}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            post.Status = request.Status;

            if (!post.Save())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Update Failed",
                    Detail = "Failed to update post status.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }

            return Ok(new AdminPostResponse
            {
                PostID = post.PostID ?? 0,
                UserID = post.UserID,
                CategoryID = post.CategoryID,
                PostTitle = post.PostTitle,
                PostDescription = post.PostDescription,
                Price = post.Price,
                Status = post.Status,
                StatusName = GetPostStatusName(post.Status),
                CreatedAt = post.CreatedAt,
                IsDeleted = post.IsDeleted
            });
        }

        /// <summary>
        /// Deletes a post (Admin override)
        /// </summary>
        [HttpDelete("posts/{id:int}")]
        [EndpointSummary("Deletes a post (Admin)")]
        [EndpointDescription("Soft deletes any post regardless of owner.")]
        [EndpointName("AdminDeletePost")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public IActionResult DeletePost(int id)
        {
            if (!Post.DeletePost(id))
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Post Not Found",
                    Detail = $"No post found with ID '{id}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }

        #endregion

        #region Category Management

        /// <summary>
        /// Gets all categories
        /// </summary>
        [HttpGet("categories")]
        [EndpointSummary("Gets all categories (Admin)")]
        [EndpointDescription("Returns all categories including deleted ones.")]
        [EndpointName("AdminGetAllCategories")]
        [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<CategoryResponse>> GetAllCategories()
        {
            var categoriesTable = Category.GetAllTbItemCategories();

            if (categoriesTable == null || categoriesTable.Rows.Count == 0)
            {
                return Ok(new List<CategoryResponse>());
            }

            var categories = new List<CategoryResponse>();

            foreach (DataRow row in categoriesTable.Rows)
            {
                categories.Add(new CategoryResponse
                {
                    CategoryID = (int)row["CategoryID"],
                    CategoryName = (string)row["CategoryName"],
                    CreatedAt = (DateTime)row["CreatedAt"],
                    IsDeleted = (bool)row["IsDeleted"]
                });
            }

            return Ok(categories);
        }

        #endregion

        #region Role Management

        /// <summary>
        /// Gets all roles
        /// </summary>
        [HttpGet("roles")]
        [EndpointSummary("Gets all roles (Admin)")]
        [EndpointDescription("Returns all system roles.")]
        [EndpointName("AdminGetAllRoles")]
        [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<RoleResponse>> GetAllRoles()
        {
            var rolesTable = Role.GetAllTbRoles();

            if (rolesTable == null || rolesTable.Rows.Count == 0)
            {
                return Ok(new List<RoleResponse>());
            }

            var roles = new List<RoleResponse>();

            foreach (DataRow row in rolesTable.Rows)
            {
                roles.Add(new RoleResponse
                {
                    RoleID = (int)row["RoleID"],
                    RoleName = (string)row["RoleName"],
                    CreatedAt = (DateTime)row["CreatedAt"],
                    IsDeleted = (bool)row["IsDeleted"]
                });
            }

            return Ok(roles);
        }

        #endregion

        #region Helper Methods

        private static string GetRoleNameFromId(int roleId)
        {
            return roleId switch
            {
                1 => RoleNames.Admin,
                2 => RoleNames.Moderator,
                _ => RoleNames.User
            };
        }

        private static string GetPostStatusName(int status)
        {
            return status switch
            {
                0 => "Draft",
                1 => "PendingReview",
                2 => "Active",
                3 => "Sold",
                4 => "Expired",
                5 => "Rejected",
                6 => "Removed",
                7 => "Deleted",
                _ => "Unknown"
            };
        }

        #endregion
    }
}
