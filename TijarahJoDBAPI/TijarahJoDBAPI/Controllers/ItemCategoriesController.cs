using Microsoft.AspNetCore.Mvc;
using Models;
using System.Collections.Generic;
using TijarahJoDB.BLL;

namespace TijarahJoDBAPI.Controllers
{
	[ApiController]
	[Route("api/TbItemCategories")]
	public class ItemCategoriesController : ControllerBase
	{

		[HttpGet("All", Name = "GetAllTbItemCategories")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<IEnumerable<CategoryModel>> GetAllTbItemCategories()
		{
			var tbitemcategoriesList = Category.GetAllTbItemCategories();

			if (tbitemcategoriesList == null || tbitemcategoriesList.Rows.Count == 0)
			{
				return NotFound("No TbItemCategories Found!");
			}

			var dtoList = new List<CategoryModel>();

			foreach (System.Data.DataRow row in tbitemcategoriesList.Rows)
			{
				dtoList.Add(new CategoryModel
				(
					(int?)row["CategoryID"],
					(string)row["CategoryName"],
					(DateTime)row["CreatedAt"],
					(bool)row["IsDeleted"]
				));
			}

			return Ok(dtoList);
		}

		[HttpGet("{id}", Name = "GetCategoryById")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<CategoryModel> GetCategoryById(int id)
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

			CategoryModel dto = category.CategoryModel;

			return Ok(dto);
		}

		[HttpPost(Name = "AddCategory")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ActionResult<CategoryModel> AddCategory(CategoryModel newCategoryDTO)
		{
			if (newCategoryDTO == null || string.IsNullOrEmpty(newCategoryDTO.CategoryName))
			{
				return BadRequest("Invalid Category data.");
			}

			Category category = new Category(new CategoryModel
			(
					newCategoryDTO.CategoryID,
					newCategoryDTO.CategoryName,
					newCategoryDTO.CreatedAt,
					newCategoryDTO.IsDeleted
			));

			if (!category.Save())
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Error adding Category");
			}

			newCategoryDTO.CategoryID = category.CategoryID;

			return CreatedAtRoute("GetCategoryById", new { id = newCategoryDTO.CategoryID }, newCategoryDTO);
		}

		[HttpPut("{id}", Name = "UpdateCategory")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<CategoryModel> UpdateCategory(int id, CategoryModel updatedCategory)
		{
			if (id < 1 || updatedCategory == null || string.IsNullOrEmpty(updatedCategory.CategoryName))
			{
				return BadRequest("Invalid Category data.");
			}

			Category category = Category.Find(id);

			if (category == null)
			{
				return NotFound($"Category with ID {id} not found.");
			}

			category.CategoryName = updatedCategory.CategoryName;
			category.CreatedAt = updatedCategory.CreatedAt;
			category.IsDeleted = updatedCategory.IsDeleted;

			if (!category.Save())
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Error updating Category");
			}

			return Ok(category.CategoryModel);
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
