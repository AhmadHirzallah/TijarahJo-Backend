using System;
using System.Data;
using TijarahJoDB.DAL;
using Models;

namespace TijarahJoDB.BLL
{
	public class Post
	{
		public enum enMode { AddNew = 0, Update = 1 };
		public enMode Mode = enMode.AddNew;

		public PostModel PostModel
		{
			get { return new PostModel(PostID = this.PostID,
                              UserID = this.UserID,
                              CategoryID = this.CategoryID,
                              PostTitle = this.PostTitle,
                              PostDescription = this.PostDescription,
                              Price = this.Price,
                              Status = this.Status,
                              CreatedAt = this.CreatedAt,
                              IsDeleted = this.IsDeleted); }
		}

		public int? PostID { set; get; }
		public int UserID { set; get; }
		public int CategoryID { set; get; }
		public string PostTitle { set; get; }
		public string PostDescription { set; get; }
		public decimal? Price { set; get; }
		public int Status { set; get; }
		public DateTime CreatedAt { set; get; }
		public bool IsDeleted { set; get; }

		public Post(PostModel PostModel, enMode cMode = enMode.AddNew)
		{
			this.PostID = PostModel.PostID;
			this.UserID = PostModel.UserID;
			this.CategoryID = PostModel.CategoryID;
			this.PostTitle = PostModel.PostTitle;
			this.PostDescription = PostModel.PostDescription;
			this.Price = PostModel.Price;
			this.Status = PostModel.Status;
			this.CreatedAt = PostModel.CreatedAt;
			this.IsDeleted = PostModel.IsDeleted;
			Mode = cMode;
		}

		private bool _AddNewPost()
		{
			this.PostID = (int?)PostData.AddPost(PostModel);
			return (this.PostID != -1);
		}

		private bool _UpdatePost()
		{
			return PostData.UpdatePost(PostModel);
		}

		public static Post Find(int? PostID)
		{
			PostModel PostModel = PostData.GetPostByID(PostID);

			if (PostModel != null)
				return new Post(PostModel, enMode.Update);
			else
				return null;
		}

		public bool Save()
		{
			switch (Mode)
			{
				case enMode.AddNew:
					if (_AddNewPost())
					{
						Mode = enMode.Update;
						return true;
					}
					else
					{
						return false;
					}

				case enMode.Update:
					return _UpdatePost();
			}
			return false;
		}
		public static bool DeletePost(int? PostID)
			=> PostData.DeletePost(PostID);
		public static bool DoesPostExist(int? PostID)
			=> PostData.DoesPostExist(PostID);
		public static DataTable GetAllTbUserPosts()
			=> PostData.GetAllTbPosts();

		public static DataTable GetPaginated(
			int PageNumber,
			int RowsPerPage = 10,
			bool IncludeDeleted = false,
			int? CategoryID = null)
			=> PostData.GetPaginated(PageNumber, RowsPerPage, IncludeDeleted, CategoryID);

		/// <summary>
		/// Gets all posts for a specific user
		/// </summary>
		public static DataTable GetPostsByUserId(int userId, bool includeDeleted = false)
			=> PostData.GetPostsByUserId(userId, includeDeleted);

		/// <summary>
		/// Gets paginated posts for a specific user
		/// </summary>
		public static DataTable GetPostsByUserIdPaginated(
			int userId,
			int pageNumber = 1,
			int rowsPerPage = 10,
			bool includeDeleted = false)
			=> PostData.GetPostsByUserIdPaginated(userId, pageNumber, rowsPerPage, includeDeleted);
	}
}
