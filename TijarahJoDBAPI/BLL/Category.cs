using System;
using System.Data;
using TijarahJoDB.DAL;
using Models;

namespace TijarahJoDB.BLL
{
	public class Category
	{
		public enum enMode { AddNew = 0, Update = 1 };
		public enMode Mode = enMode.AddNew;

		public CategoryModel CategoryModel
		{
			get { return new CategoryModel(CategoryID = this.CategoryID, CategoryName = this.CategoryName, CreatedAt = this.CreatedAt, IsDeleted = this.IsDeleted); }
		}

		public int? CategoryID { set; get; }
		public string CategoryName { set; get; }
		public DateTime CreatedAt { set; get; }
		public bool IsDeleted { set; get; }

		public Category(CategoryModel CategoryModel, enMode cMode = enMode.AddNew)
		{
			this.CategoryID = CategoryModel.CategoryID;
			this.CategoryName = CategoryModel.CategoryName;
			this.CreatedAt = CategoryModel.CreatedAt;
			this.IsDeleted = CategoryModel.IsDeleted;
			Mode = cMode;
		}

		private bool _AddNewCategory()
		{
			this.CategoryID = (int?)CategoryData.AddCategory(CategoryModel);
			return (this.CategoryID != -1);
		}

		private bool _UpdateCategory()
		{
			return CategoryData.UpdateCategory(CategoryModel);
		}

		public static Category Find(int? CategoryID)
		{
			CategoryModel CategoryModel = CategoryData.GetCategoryByID(CategoryID);

			if (CategoryModel != null)
				return new Category(CategoryModel, enMode.Update);
			else
				return null;
		}

		public bool Save()
		{
			switch (Mode)
			{
				case enMode.AddNew:
					if (_AddNewCategory())
					{
						Mode = enMode.Update;
						return true;
					}
					else
					{
						return false;
					}

				case enMode.Update:
					return _UpdateCategory();
			}
			return false;
		}
		public static bool DeleteCategory(int? CategoryID)
			=> CategoryData.DeleteCategory(CategoryID);
		public static bool DoesCategoryExist(int? CategoryID)
			=> CategoryData.DoesCategoryExist(CategoryID);
		public static DataTable GetAllTbItemCategories()
			=> CategoryData.GetAllTbItemCategories();
	}
}
