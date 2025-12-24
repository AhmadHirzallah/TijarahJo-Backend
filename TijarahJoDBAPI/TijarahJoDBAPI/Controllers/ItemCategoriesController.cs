using Microsoft.AspNetCore.Mvc;
using Models;
using TijarahJoDB.BLL;
using TijarahJoDBAPI.DTOs.Requests;
using TijarahJoDBAPI.DTOs.Responses;
using TijarahJoDBAPI.DTOs.Mappers;

namespace TijarahJoDBAPI.Controllers
{
    [ApiController]
    [Route("api/TbItemCategories")]
    public class ItemCategoriesController : ControllerBase
    {
        [HttpGet("All", Name = "GetAllTbItemCategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<CategoryResponse>> GetAllTbItemCategories()
        {
            var tbitemcategoriesList = Category.GetAllTbItemCategories();

            if (tbitemcategoriesList == null || tbitemcategoriesList.Rows.Count == 0)
            {
                return NotFound("No TbItemCategories Found!");
            }

            var responseList = new List<CategoryResponse>();

            foreach (System.Data.DataRow row in tbitemcategoriesList.Rows)
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

        [HttpGet("{id}", Name = "GetCategoryById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CategoryResponse> GetCategoryById(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            Category category = Category.Find(id);

            if (category == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }

            return Ok(category.CategoryModel.ToResponse());
        }

        [HttpPost(Name = "AddCategory")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<CategoryResponse> AddCategory([FromBody] CreateCategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Category category = new Category(new CategoryModel
            (
                null, // CategoryID - will be generated
                request.CategoryName,
                DateTime.UtcNow, // CreatedAt
                false // IsDeleted
            ));

            if (!category.Save())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding Category");
            }

            var response = category.CategoryModel.ToResponse();

            return CreatedAtRoute("GetCategoryById", new { id = response.CategoryID }, response);
        }

        [HttpPut("{id}", Name = "UpdateCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CategoryResponse> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Category category = Category.Find(id);

            if (category == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }

            category.CategoryName = request.CategoryName;
            category.IsDeleted = request.IsDeleted;

            if (!category.Save())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating Category");
            }

            return Ok(category.CategoryModel.ToResponse());
        }

        [HttpDelete("{id}", Name = "DeleteCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteCategory(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            if (Category.DeleteCategory(id))
            {
                return Ok($"Category with ID {id} has been deleted.");
            }
            else
            {
                return NotFound($"Category with ID {id} not found. No rows deleted!");
            }
        }

        [HttpGet("Exists/{id}", Name = "DoesCategoryExist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<bool> DoesCategoryExist(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            bool exists = Category.DoesCategoryExist(id);

            return Ok(exists);
        }
    }
}
