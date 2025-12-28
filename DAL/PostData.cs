using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;
using TijarahJoDB_DataAccess;

namespace TijarahJoDB.DAL
{
	public class PostData
	{
		public static PostModel GetPostByID(int? PostID)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_GetPostByID", connection))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("@PostID", PostID);

						connection.Open();

						using (SqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								return new PostModel
								(
							reader.GetInt32(reader.GetOrdinal("PostID")),
							reader.GetInt32(reader.GetOrdinal("UserID")),
							reader.GetInt32(reader.GetOrdinal("CategoryID")),
							reader.GetString(reader.GetOrdinal("PostTitle")),
							reader.IsDBNull(reader.GetOrdinal("PostDescription")) ? null : (string)reader.GetString(reader.GetOrdinal("PostDescription")),
							reader.IsDBNull(reader.GetOrdinal("Price")) ? null : (decimal?)reader.GetDecimal(reader.GetOrdinal("Price")),
							reader.GetInt32(reader.GetOrdinal("Status")),
							reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
							reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
								);
							}
							else
							{
								return null;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				// Log exception
				return null;
			}
		}

		public static int AddPost(PostModel PostModel)
		{
			try
			{
				using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				using (var command = new SqlCommand("SP_AddPost", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					command.Parameters.AddWithValue("@UserID", PostModel.UserID);
					command.Parameters.AddWithValue("@CategoryID", PostModel.CategoryID);
					command.Parameters.AddWithValue("@PostTitle", PostModel.PostTitle);
					command.Parameters.AddWithValue("@PostDescription", (object?)PostModel.PostDescription ?? DBNull.Value);
					command.Parameters.AddWithValue("@Price", (object?)PostModel.Price ?? DBNull.Value);
					command.Parameters.AddWithValue("@Status", PostModel.Status);
					command.Parameters.AddWithValue("@CreatedAt", PostModel.CreatedAt);
					command.Parameters.AddWithValue("@IsDeleted", PostModel.IsDeleted);
					var outputIdParam = new SqlParameter("@NewPostID", SqlDbType.Int)
					{
						Direction = ParameterDirection.Output
					};
					command.Parameters.Add(outputIdParam);

					connection.Open();
					command.ExecuteNonQuery();

					return (int)outputIdParam.Value;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"AddPost Error: {ex.Message}");
				return -1;
			}
		}

		public static bool UpdatePost(PostModel dto)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_UpdatePost", connection))
					{
						command.CommandType = CommandType.StoredProcedure;

						command.Parameters.AddWithValue("@PostID", dto.PostID);
						command.Parameters.AddWithValue("@UserID", dto.UserID);
						command.Parameters.AddWithValue("@CategoryID", dto.CategoryID);
						command.Parameters.AddWithValue("@PostTitle", dto.PostTitle);
						command.Parameters.AddWithValue("@PostDescription", (object?)dto.PostDescription ?? DBNull.Value);
						command.Parameters.AddWithValue("@Price", (object?)dto.Price ?? DBNull.Value);
						command.Parameters.AddWithValue("@Status", dto.Status);
						command.Parameters.AddWithValue("@CreatedAt", dto.CreatedAt);
						command.Parameters.AddWithValue("@IsDeleted", dto.IsDeleted);

						connection.Open();
						
						// Use ExecuteNonQuery - returns number of rows affected
						int rowsAffected = command.ExecuteNonQuery();
						return rowsAffected > 0;
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"UpdatePost Error: {ex.Message}");
				return false;
			}
		}

		public static bool DeletePost(int? PostID)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					// Use direct SQL instead of stored procedure for more control
					using (SqlCommand command = new SqlCommand(@"
						UPDATE TbPosts 
						SET IsDeleted = 1 
						WHERE PostID = @PostID AND IsDeleted = 0;
						SELECT @@ROWCOUNT;", connection))
					{
						command.CommandType = CommandType.Text;
						command.Parameters.AddWithValue("@PostID", PostID);

						connection.Open();
						var result = command.ExecuteScalar();
						int rowsAffected = result != null ? (int)result : 0;
						return rowsAffected > 0;
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"DeletePost Error: {ex.Message}");
				return false;
			}
		}

		public static bool DoesPostExist(int? PostID)
		{
			bool isFound = false;

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_DoesPostExist", connection))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("@PostID", PostID);

						connection.Open();

						object result = command.ExecuteScalar();
						if (result != null && result != DBNull.Value)
							isFound = (bool)result;
					}
				}
			}
			catch (Exception ex)
			{
				// Log exception
				isFound = false;
			}

			return isFound;
		}

		public static DataTable GetAllTbPosts()
		{
			DataTable dt = new DataTable();

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_GetAllTbPosts", connection))
					{
						command.CommandType = CommandType.StoredProcedure;

						connection.Open();

						using (SqlDataReader reader = command.ExecuteReader())
						{
							if (reader.HasRows)
								dt.Load(reader);
						}
					}
				}
			}
			catch (Exception ex)
			{
				// Log exception
			}

			return dt;
		}


		/*
		@IncludeDeleted
		@RowsPerPage
		@PageNumber
		
		*/
		/// <summary>
		/// Gets paginated posts with optional category filtering.
		/// Returns enriched data from VW_PostsForListing view including user and category details.
		/// </summary>
		/// <param name="pageNumber">Page number (1-based)</param>
		/// <param name="rowsPerPage">Number of rows per page (default: 10, max: 200)</param>
		/// <param name="includeDeleted">Include soft-deleted posts</param>
		/// <param name="categoryId">Optional category filter (null = all categories)</param>
		/// <returns>DataTable with posts and TotalRows column</returns>
		public static DataTable GetPaginated(
			int pageNumber,
			int rowsPerPage = 10,
			bool includeDeleted = false,
			int? categoryId = null)
		{
			DataTable dt = new DataTable();

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				using (SqlCommand command = new SqlCommand("SP_GetTbPostsPaged", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					// Use typed parameters for better performance and type safety
					command.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
					command.Parameters.Add("@RowsPerPage", SqlDbType.Int).Value = rowsPerPage;
					command.Parameters.Add("@IncludeDeleted", SqlDbType.Bit).Value = includeDeleted;

					// Handle nullable CategoryID
					var categoryParam = command.Parameters.Add("@CategoryID", SqlDbType.Int);
					categoryParam.Value = categoryId.HasValue ? categoryId.Value : DBNull.Value;

					connection.Open();

					using (SqlDataReader reader = command.ExecuteReader())
					{
						if (reader.HasRows)
							dt.Load(reader);
					}
				}
			}
			catch (Exception ex)
			{
				// Log exception
				// Consider using ILogger in production
			}

			return dt;
		}

		/// <summary>
		/// Gets all posts for a specific user
		/// </summary>
		/// <param name="userId">The user ID</param>
		/// <param name="includeDeleted">Include soft-deleted posts</param>
		/// <returns>DataTable with user's posts</returns>
		public static DataTable GetPostsByUserId(int userId, bool includeDeleted = false)
		{
			DataTable dt = new DataTable();

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				using (SqlCommand command = new SqlCommand(@"
					SELECT 
						p.PostID,
						p.UserID,
						p.CategoryID,
						p.PostTitle,
						p.PostDescription,
						p.Price,
						p.Status,
						p.CreatedAt,
						p.IsDeleted,
						c.CategoryName,
						(SELECT TOP 1 PostImageURL FROM TbPostImages WHERE PostID = p.PostID AND IsDeleted = 0 ORDER BY UploadedAt) AS PrimaryImageUrl
					FROM TbPosts p
					LEFT JOIN TbItemCategories c ON p.CategoryID = c.CategoryID
					WHERE p.UserID = @UserID
						AND (@IncludeDeleted = 1 OR p.IsDeleted = 0)
					ORDER BY p.CreatedAt DESC", connection))
				{
					command.CommandType = CommandType.Text;
					command.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
					command.Parameters.Add("@IncludeDeleted", SqlDbType.Bit).Value = includeDeleted;

					connection.Open();

					using (SqlDataReader reader = command.ExecuteReader())
					{
						if (reader.HasRows)
							dt.Load(reader);
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"GetPostsByUserId Error: {ex.Message}");
			}

			return dt;
		}

		/// <summary>
		/// Gets paginated posts for a specific user
		/// </summary>
		public static DataTable GetPostsByUserIdPaginated(
			int userId,
			int pageNumber = 1,
			int rowsPerPage = 10,
			bool includeDeleted = false)
		{
			DataTable dt = new DataTable();

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				using (SqlCommand command = new SqlCommand(@"
					;WITH PostsCTE AS (
						SELECT 
							p.PostID,
							p.UserID,
							p.CategoryID,
							p.PostTitle,
							p.PostDescription,
							p.Price,
							p.Status,
							p.CreatedAt,
							p.IsDeleted,
							c.CategoryName,
							(SELECT TOP 1 PostImageURL FROM TbPostImages WHERE PostID = p.PostID AND IsDeleted = 0 ORDER BY UploadedAt) AS PrimaryImageUrl,
							COUNT(*) OVER() AS TotalRows
						FROM TbPosts p
						LEFT JOIN TbItemCategories c ON p.CategoryID = c.CategoryID
						WHERE p.UserID = @UserID
							AND (@IncludeDeleted = 1 OR p.IsDeleted = 0)
					)
					SELECT *
					FROM PostsCTE
					ORDER BY CreatedAt DESC
					OFFSET (@PageNumber - 1) * @RowsPerPage ROWS
					FETCH NEXT @RowsPerPage ROWS ONLY", connection))
				{
					command.CommandType = CommandType.Text;
					command.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
					command.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
					command.Parameters.Add("@RowsPerPage", SqlDbType.Int).Value = rowsPerPage;
					command.Parameters.Add("@IncludeDeleted", SqlDbType.Bit).Value = includeDeleted;

					connection.Open();

					using (SqlDataReader reader = command.ExecuteReader())
					{
						if (reader.HasRows)
							dt.Load(reader);
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"GetPostsByUserIdPaginated Error: {ex.Message}");
			}

			return dt;
		}
	}
}
