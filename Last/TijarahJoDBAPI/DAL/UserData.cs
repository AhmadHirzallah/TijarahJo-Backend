using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;
using TijarahJoDB_DataAccess;

namespace TijarahJoDB.DAL
{
	public class UserData
	{
		public static UserModel GetUserByID(int? UserID)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_GetUserByID", connection))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("@UserID", UserID);

						connection.Open();

						using (SqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								return new UserModel
								(
							reader.GetInt32(reader.GetOrdinal("UserID")),
							reader.GetString(reader.GetOrdinal("Username")),
							reader.GetString(reader.GetOrdinal("HashedPassword")),
							reader.GetString(reader.GetOrdinal("Email")),
							reader.GetString(reader.GetOrdinal("FirstName")),
							reader.IsDBNull(reader.GetOrdinal("LastName")) ? null : (string)reader.GetString(reader.GetOrdinal("LastName")),
							reader.GetDateTime(reader.GetOrdinal("JoinDate")),
							reader.GetInt32(reader.GetOrdinal("Status")),
							reader.GetInt32(reader.GetOrdinal("RoleID")),
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

		public static int AddUser(UserModel UserModel)
		{
			using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
			using (var command = new SqlCommand("SP_AddUser", connection))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.AddWithValue("@Username", UserModel.Username);
				command.Parameters.AddWithValue("@HashedPassword", UserModel.HashedPassword);
				command.Parameters.AddWithValue("@Email", UserModel.Email);
				command.Parameters.AddWithValue("@FirstName", UserModel.FirstName);
				command.Parameters.AddWithValue("@LastName", (object)UserModel.LastName ?? DBNull.Value);
				command.Parameters.AddWithValue("@JoinDate", UserModel.JoinDate);
				command.Parameters.AddWithValue("@Status", UserModel.Status);
				command.Parameters.AddWithValue("@RoleID", UserModel.RoleID);
				command.Parameters.AddWithValue("@IsDeleted", UserModel.IsDeleted);
				var outputIdParam = new SqlParameter("@NewUserID", SqlDbType.Int)
				{
					Direction = ParameterDirection.Output
				};
				command.Parameters.Add(outputIdParam);

				connection.Open();
				command.ExecuteNonQuery();

				return (int)outputIdParam.Value;
			}
		}

		public static bool UpdateUser(UserModel dto)
		{
			int rowsAffected = 0;

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_UpdateUser", connection))
					{
						command.CommandType = CommandType.StoredProcedure;

						command.Parameters.AddWithValue("@UserID", dto.UserID);
						command.Parameters.AddWithValue("@Username", dto.Username);
						command.Parameters.AddWithValue("@HashedPassword", dto.HashedPassword);
						command.Parameters.AddWithValue("@Email", dto.Email);
						command.Parameters.AddWithValue("@FirstName", dto.FirstName);
						command.Parameters.AddWithValue("@LastName", (object)dto.LastName ?? DBNull.Value);
						command.Parameters.AddWithValue("@JoinDate", dto.JoinDate);
						command.Parameters.AddWithValue("@Status", dto.Status);
						command.Parameters.AddWithValue("@RoleID", dto.RoleID);
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

		public static bool DeleteUser(int? UserID)
		{
			int rowsAffected = 0;

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_DeleteUser", connection))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("@UserID", UserID);

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

		public static bool DoesUserExist(int? UserID)
		{
			bool isFound = false;

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_DoesUserExist", connection))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("@UserID", UserID);

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

		public static DataTable GetAllTbUsers()
		{
			DataTable dt = new DataTable();

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_GetAllTbUsers", connection))
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
	}
}
