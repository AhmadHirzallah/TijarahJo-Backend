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
			using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
			using (var command = new SqlCommand("SP_AddPost", connection))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.AddWithValue("@UserID", PostModel.UserID);
				command.Parameters.AddWithValue("@CategoryID", PostModel.CategoryID);
				command.Parameters.AddWithValue("@PostTitle", PostModel.PostTitle);
				command.Parameters.AddWithValue("@PostDescription", (object)PostModel.PostDescription ?? DBNull.Value);
				command.Parameters.AddWithValue("@Price", (object)PostModel.Price ?? DBNull.Value);
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

		public static bool UpdatePost(PostModel dto)
		{
			int rowsAffected = 0;

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
						command.Parameters.AddWithValue("@PostDescription", (object)dto.PostDescription ?? DBNull.Value);
						command.Parameters.AddWithValue("@Price", (object)dto.Price ?? DBNull.Value);
						command.Parameters.AddWithValue("@Status", dto.Status);
						command.Parameters.AddWithValue("@CreatedAt", dto.CreatedAt);
						command.Parameters.AddWithValue("@IsDeleted", dto.IsDeleted);

						connection.Open();
						rowsAffected = (int)command.ExecuteScalar();
					}
				}
			}
			catch (Exception ex)
			{
				// Log exception
				return false;
			}

			return (rowsAffected > 0);
		}

		public static bool DeletePost(int? PostID)
		{
			int rowsAffected = 0;

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_DeletePost", connection))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("@PostID", PostID);

						connection.Open();
						rowsAffected = (int)command.ExecuteScalar();
					}
				}
			}
			catch (Exception ex)
			{
				// Log exception
			}

			return (rowsAffected > 0);
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
		public static DataTable GetPaginated(
			int PageNumber,
			int RowsPerPage = 10,
			bool IncludeDeleted = false)
		{
			DataTable dt = new DataTable();

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_GetTbPostsPaged", connection))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("@PageNumber", PageNumber);
						command.Parameters.AddWithValue("@RowsPerPage", RowsPerPage);
						command.Parameters.AddWithValue("@IncludeDeleted", IncludeDeleted);

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

	}

}
