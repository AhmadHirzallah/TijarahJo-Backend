using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;
using TijarahJoDB_DataAccess;

namespace TijarahJoDB.DAL
{
	public class PostImageData
	{
		public static PostImageModel GetPostImageByID(int? PostImageID)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_GetPostImageByID", connection))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("@PostImageID", PostImageID);

						connection.Open();

						using (SqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								return new PostImageModel
								(
							reader.GetInt32(reader.GetOrdinal("PostImageID")),
							reader.GetInt32(reader.GetOrdinal("PostID")),
							reader.GetString(reader.GetOrdinal("PostImageURL")),
							reader.GetDateTime(reader.GetOrdinal("UploadedAt")),
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

		public static int AddPostImage(PostImageModel PostImageModel)
		{
			using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
			using (var command = new SqlCommand("SP_AddPostImage", connection))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.AddWithValue("@PostID", PostImageModel.PostID);
				command.Parameters.AddWithValue("@PostImageURL", PostImageModel.PostImageURL);
				command.Parameters.AddWithValue("@UploadedAt", PostImageModel.UploadedAt);
				command.Parameters.AddWithValue("@IsDeleted", PostImageModel.IsDeleted);
				var outputIdParam = new SqlParameter("@NewPostImageID", SqlDbType.Int)
				{
					Direction = ParameterDirection.Output
				};
				command.Parameters.Add(outputIdParam);

				connection.Open();
				command.ExecuteNonQuery();

				return (int)outputIdParam.Value;
			}
		}

		public static bool UpdatePostImage(PostImageModel dto)
		{
			int rowsAffected = 0;

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_UpdatePostImage", connection))
					{
						command.CommandType = CommandType.StoredProcedure;

						command.Parameters.AddWithValue("@PostImageID", dto.PostImageID);
						command.Parameters.AddWithValue("@PostID", dto.PostID);
						command.Parameters.AddWithValue("@PostImageURL", dto.PostImageURL);
						command.Parameters.AddWithValue("@UploadedAt", dto.UploadedAt);
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

		public static bool DeletePostImage(int? PostImageID)
		{
			int rowsAffected = 0;

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_DeletePostImage", connection))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("@PostImageID", PostImageID);

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

		public static bool DoesPostImageExist(int? PostImageID)
		{
			bool isFound = false;

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_DoesPostImageExist", connection))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("@PostImageID", PostImageID);

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

		public static DataTable GetAllTbPostImages()
		{
			DataTable dt = new DataTable();

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_GetAllTbPostImages", connection))
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

		/// <summary>
		/// Gets all images for a specific post
		/// </summary>
		public static DataTable GetImagesByPostId(int postId)
		{
			DataTable dt = new DataTable();

			try
			{
				using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
				using var command = new SqlCommand("SP_GetPostImagesByPostID", connection);

				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add("@PostID", SqlDbType.Int).Value = postId;

				connection.Open();

				using var reader = command.ExecuteReader();
				if (reader.HasRows)
					dt.Load(reader);
			}
			catch (Exception ex)
			{
				// Log exception
			}

			return dt;
		}
	}
}
