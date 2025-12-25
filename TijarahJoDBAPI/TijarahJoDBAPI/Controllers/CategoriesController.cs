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
    [Route("api/categories")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Retrieves all categories
        /// </summary>
        /// <returns>A list of all item categories</returns>
        [HttpGet]
        [EndpointSummary("Retrieves all categories")]
        [EndpointDescription("Returns a list of all item categories available in the marketplace.")]
        [EndpointName("GetAllCategories")]
        [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<CategoryResponse>> GetAll()
        {
            var categoriesList = Category.GetAllTbItemCategories();

            if (categoriesList == null || categoriesList.Rows.Count == 0)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "No Categories Found",
                    Detail = "There are no categories available in the system.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var responseList = new List<CategoryResponse>();

            foreach (System.Data.DataRow row in categoriesList.Rows)
            {
                responseList.Add(new CategoryResponse
                {
                    CategoryID = (int?)row["CategoryID"],
                    CategoryName = (string)row["CategoryName"],
                    CreatedAt = (DateTime)row["CreatedAt"],
                    IsDeleted = (bool)row["IsDeleted"]
                });
            }

            return Ok(responseList);
        }

        /// <summary>
        /// Retrieves a category by ID
        /// </summary>
        /// <param name="id">The category ID</param>
        /// <returns>The category details</returns>
        [HttpGet("{id:int}", Name = "GetCategoryById")]
        [EndpointSummary("Retrieves a category by ID")]
        [EndpointDescription("Returns detailed information about a specific item category.")]
        [EndpointName("GetCategoryById")]
        [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult<CategoryResponse> GetById(int id)
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

            Category category = Category.Find(id);

            if (category == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Category Not Found",
                    Detail = $"No category found with ID '{id}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(category.CategoryModel.ToResponse());
        }

        /// <summary>
        /// Checks if a category exists
        /// </summary>
        /// <param name="id">The category ID to check</param>
        /// <returns>Boolean indicating existence</returns>
        [HttpGet("{id:int}/exists")]
        [EndpointSummary("Checks if a category exists")]
        [EndpointDescription("Returns true if a category with the specified ID exists, false otherwise.")]
        [EndpointName("CheckCategoryExists")]
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

            return Ok(Category.DoesCategoryExist(id));
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Creates a new category (Admin only)
        /// </summary>
        /// <param name="request">The category data</param>
        /// <returns>The created category</returns>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [EndpointSummary("Creates a new category (Admin only)")]
        [EndpointDescription("Adds a new item category to the marketplace. Requires Admin privileges.")]
        [EndpointName("CreateCategory")]
        [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public ActionResult<CategoryResponse> Create([FromBody] CreateCategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Category category = new Category(new CategoryModel
            (
                null,
                request.CategoryName,
                DateTime.UtcNow,
                false
            ));

            if (!category.Save())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Creation Failed",
                    Detail = "An error occurred while creating the category.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }

            var response = category.CategoryModel.ToResponse();

            return CreatedAtRoute("GetCategoryById", new { id = response.CategoryID }, response);
        }

        #endregion

        #region PUT Endpoints

        /// <summary>
        /// Updates an existing category (Admin only)
        /// </summary>
        /// <param name="id">The category ID</param>
        /// <param name="request">The updated category data</param>
        /// <returns>The updated category</returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [EndpointSummary("Updates an existing category (Admin only)")]
        [EndpointDescription("Updates the details of an existing item category. Requires Admin privileges.")]
        [EndpointName("UpdateCategory")]
        [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public ActionResult<CategoryResponse> Update(int id, [FromBody] UpdateCategoryRequest request)
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

            Category category = Category.Find(id);

            if (category == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Category Not Found",
                    Detail = $"No category found with ID '{id}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            category.CategoryName = request.CategoryName;
            category.IsDeleted = request.IsDeleted;

            if (!category.Save())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Update Failed",
                    Detail = "An error occurred while updating the category.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }

            return Ok(category.CategoryModel.ToResponse());
        }

        #endregion

        #region DELETE Endpoints

        /// <summary>
        /// Deletes a category (Admin only)
        /// </summary>
        /// <param name="id">The category ID to delete</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [EndpointSummary("Deletes a category (Admin only)")]
        [EndpointDescription("Soft deletes an item category from the marketplace. Requires Admin privileges.")]
        [EndpointName("DeleteCategory")]
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

            if (!Category.DeleteCategory(id))
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Category Not Found",
                    Detail = $"No category found with ID '{id}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }

        #endregion
    }
}
