using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;
using TijarahJoDB_DataAccess;

namespace TijarahJoDB.DAL
{
	public class RoleData
	{
		public static RoleModel GetRoleByID(int? RoleID)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_GetRoleByID", connection))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("@RoleID", RoleID);

						connection.Open();

						using (SqlDataReader reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								return new RoleModel
								(
							reader.GetInt32(reader.GetOrdinal("RoleID")),
							reader.GetString(reader.GetOrdinal("RoleName")),
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

		public static int AddRole(RoleModel RoleModel)
		{
			using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
			using (var command = new SqlCommand("SP_AddRole", connection))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.AddWithValue("@RoleName", RoleModel.RoleName);
				command.Parameters.AddWithValue("@CreatedAt", RoleModel.CreatedAt);
				command.Parameters.AddWithValue("@IsDeleted", RoleModel.IsDeleted);
				var outputIdParam = new SqlParameter("@NewRoleID", SqlDbType.Int)
				{
					Direction = ParameterDirection.Output
				};
				command.Parameters.Add(outputIdParam);

				connection.Open();
				command.ExecuteNonQuery();

				return (int)outputIdParam.Value;
			}
		}

		public static bool UpdateRole(RoleModel dto)
		{
			int rowsAffected = 0;

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_UpdateRole", connection))
					{
						command.CommandType = CommandType.StoredProcedure;

						command.Parameters.AddWithValue("@RoleID", dto.RoleID);
						command.Parameters.AddWithValue("@RoleName", dto.RoleName);
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

		public static bool DeleteRole(int? RoleID)
		{
			int rowsAffected = 0;

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_DeleteRole", connection))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("@RoleID", RoleID);

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

		public static bool DoesRoleExist(int? RoleID)
		{
			bool isFound = false;

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_DoesRoleExist", connection))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("@RoleID", RoleID);

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

		public static DataTable GetAllTbRoles()
		{
			DataTable dt = new DataTable();

			try
			{
				using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("SP_GetAllTbRoles", connection))
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
