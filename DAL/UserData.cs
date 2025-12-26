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
									reader.IsDBNull(reader.GetOrdinal("LastName")) ? null : reader.GetString(reader.GetOrdinal("LastName")),
									reader.GetDateTime(reader.GetOrdinal("JoinDate")),
									reader.GetInt32(reader.GetOrdinal("Status")),
									reader.GetInt32(reader.GetOrdinal("RoleID")),
									reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
									reader.IsDBNull(reader.GetOrdinal("PrimaryPhone")) ? null : reader.GetString(reader.GetOrdinal("PrimaryPhone"))
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
				System.Diagnostics.Debug.WriteLine($"GetUserByID Error: {ex.Message}");
				return null;
			}
		}

        /// <summary>
        /// Gets user by username or email for login (without password verification)
        /// Password verification should be done by the application layer
        /// </summary>
        /// <param name="login">Username or email</param>
        /// <returns>UserModel if found, null otherwise</returns>
        public static UserModel? GetUserByLogin(string login)
		{
			try
			{
				using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
				using var command = new SqlCommand("SP_GetUserByLogin", connection);

				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.AddWithValue("@Login", login);

				connection.Open();

				using var reader = command.ExecuteReader();
				if (reader.Read())
				{
					return new UserModel(
						reader.GetInt32(reader.GetOrdinal("UserID")),
						reader.GetString(reader.GetOrdinal("Username")),
						reader.GetString(reader.GetOrdinal("HashedPassword")),
						reader.GetString(reader.GetOrdinal("Email")),
						reader.GetString(reader.GetOrdinal("FirstName")),
						reader.IsDBNull(reader.GetOrdinal("LastName")) ? null : reader.GetString(reader.GetOrdinal("LastName")),
						reader.GetDateTime(reader.GetOrdinal("JoinDate")),
						reader.GetInt32(reader.GetOrdinal("Status")),
						reader.GetInt32(reader.GetOrdinal("RoleID")),
						reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
						reader.IsDBNull(reader.GetOrdinal("PrimaryPhone")) ? null : reader.GetString(reader.GetOrdinal("PrimaryPhone"))
					);
				}

				return null;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"GetUserByLogin Error: {ex.Message}");
				return null;
			}
		}

		public static int AddUser(UserModel UserModel)
		{
			try
			{
				using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
				using (var command = new SqlCommand("SP_CreateUser", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					command.Parameters.Add("@Username", SqlDbType.NVarChar, 200).Value = UserModel.Username;
					command.Parameters.Add("@HashedPassword", SqlDbType.NVarChar, 510).Value = UserModel.HashedPassword;
					command.Parameters.Add("@Email", SqlDbType.NVarChar, 510).Value = UserModel.Email;
					command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 200).Value = UserModel.FirstName;
					command.Parameters.Add("@LastName", SqlDbType.NVarChar, 200).Value = (object?)UserModel.LastName ?? DBNull.Value;
					command.Parameters.Add("@JoinDate", SqlDbType.DateTime2).Value = UserModel.JoinDate;
					command.Parameters.Add("@Status", SqlDbType.Int).Value = UserModel.Status;
					command.Parameters.Add("@RoleID", SqlDbType.Int).Value = UserModel.RoleID;
					command.Parameters.Add("@IsDeleted", SqlDbType.Bit).Value = UserModel.IsDeleted;
					
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
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"AddUser Error: {ex.Message}");
				return -1;
			}
		}

		/// <summary>
		/// Creates a user with an optional primary phone number in one transaction
		/// </summary>
		public static int AddUserWithPhone(UserModel userModel, string? phoneNumber)
		{
			try
			{
				using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
				using var command = new SqlCommand("SP_CreateUserWithPhone", connection);
				
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.Add("@Username", SqlDbType.NVarChar, 200).Value = userModel.Username;
				command.Parameters.Add("@HashedPassword", SqlDbType.NVarChar, 510).Value = userModel.HashedPassword;
				command.Parameters.Add("@Email", SqlDbType.NVarChar, 510).Value = userModel.Email;
				command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 200).Value = userModel.FirstName;
				command.Parameters.Add("@LastName", SqlDbType.NVarChar, 200).Value = (object?)userModel.LastName ?? DBNull.Value;
				command.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 20).Value = (object?)phoneNumber ?? DBNull.Value;
				command.Parameters.Add("@JoinDate", SqlDbType.DateTime2).Value = userModel.JoinDate;
				command.Parameters.Add("@Status", SqlDbType.Int).Value = userModel.Status;
				command.Parameters.Add("@RoleID", SqlDbType.Int).Value = userModel.RoleID;
				command.Parameters.Add("@IsDeleted", SqlDbType.Bit).Value = userModel.IsDeleted;
				
				var outputIdParam = new SqlParameter("@NewUserID", SqlDbType.Int)
				{
					Direction = ParameterDirection.Output
				};
				command.Parameters.Add(outputIdParam);

				connection.Open();
				command.ExecuteNonQuery();

				return (int)outputIdParam.Value;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"AddUserWithPhone Error: {ex.Message}");
				return -1;
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
						command.Parameters.AddWithValue("@LastName", (object?)dto.LastName ?? DBNull.Value);
						command.Parameters.AddWithValue("@JoinDate", dto.JoinDate);
						command.Parameters.AddWithValue("@Status", dto.Status);
						command.Parameters.AddWithValue("@RoleID", dto.RoleID);
						command.Parameters.AddWithValue("@IsDeleted", dto.IsDeleted);

						connection.Open();
						object? result = command.ExecuteScalar();
						rowsAffected = result != null ? Convert.ToInt32(result) : 0;
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"UpdateUser Error: {ex.Message}");
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
						rowsAffected = command.ExecuteNonQuery();
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"DeleteUser Error: {ex.Message}");
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
				System.Diagnostics.Debug.WriteLine($"DoesUserExist Error: {ex.Message}");
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
				System.Diagnostics.Debug.WriteLine($"GetAllTbUsers Error: {ex.Message}");
			}

			return dt;
		}

        public static UserModel GetUserByLoginAndPassword(string login, string hashedPassword)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                using (SqlCommand command = new SqlCommand("SP_TbUsers_Login", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@HashedPassword", hashedPassword);

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
                                reader.IsDBNull(reader.GetOrdinal("LastName")) ? null : reader.GetString(reader.GetOrdinal("LastName")),
                                reader.GetDateTime(reader.GetOrdinal("JoinDate")),
                                reader.GetInt32(reader.GetOrdinal("Status")),
                                reader.GetInt32(reader.GetOrdinal("RoleID")),
                                reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUserByLoginAndPassword Error: {ex.Message}");
            }

            return null;
        }
    }
}
