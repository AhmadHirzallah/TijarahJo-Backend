using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;
using TijarahJoDB_DataAccess;

namespace TijarahJoDB.DAL
{
    /// <summary>
    /// Data access layer for user images
    /// </summary>
    public class UserImageData
    {
        /// <summary>
        /// Gets a user image by its ID
        /// </summary>
        public static UserImageModel? GetUserImageByID(int? userImageId)
        {
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var command = new SqlCommand("SP_GetUserImageByID", connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@UserImageID", SqlDbType.Int).Value = userImageId;

                connection.Open();

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new UserImageModel(
                        reader.GetInt32(reader.GetOrdinal("UserImageID")),
                        reader.GetInt32(reader.GetOrdinal("UserID")),
                        reader.GetString(reader.GetOrdinal("ImageURL")),
                        reader.GetDateTime(reader.GetOrdinal("UploadedAt")),
                        reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
                    );
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log exception
                return null;
            }
        }

        /// <summary>
        /// Adds a new user image
        /// </summary>
        public static int AddUserImage(UserImageModel model)
        {
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var command = new SqlCommand("SP_AddUserImage", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@UserID", SqlDbType.Int).Value = model.UserID;
                command.Parameters.Add("@ImageURL", SqlDbType.NVarChar, 2000).Value = model.ImageURL;
                command.Parameters.Add("@UploadedAt", SqlDbType.DateTime2).Value = model.UploadedAt;
                command.Parameters.Add("@IsDeleted", SqlDbType.Bit).Value = model.IsDeleted;

                var outputIdParam = new SqlParameter("@NewUserImageID", SqlDbType.Int)
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
                // Log exception
                return -1;
            }
        }

        /// <summary>
        /// Updates an existing user image
        /// </summary>
        public static bool UpdateUserImage(UserImageModel model)
        {
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var command = new SqlCommand("SP_UpdateUserImage", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@UserImageID", SqlDbType.Int).Value = model.UserImageID;
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = model.UserID;
                command.Parameters.Add("@ImageURL", SqlDbType.NVarChar, 2000).Value = model.ImageURL;
                command.Parameters.Add("@UploadedAt", SqlDbType.DateTime2).Value = model.UploadedAt;
                command.Parameters.Add("@IsDeleted", SqlDbType.Bit).Value = model.IsDeleted;

                connection.Open();
                var rowsAffected = (int)command.ExecuteScalar();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // Log exception
                return false;
            }
        }

        /// <summary>
        /// Soft deletes a user image
        /// </summary>
        public static bool DeleteUserImage(int? userImageId)
        {
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var command = new SqlCommand("SP_DeleteUserImage", connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@UserImageID", SqlDbType.Int).Value = userImageId;

                connection.Open();
                var rowsAffected = (int)command.ExecuteScalar();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // Log exception
                return false;
            }
        }

        /// <summary>
        /// Checks if a user image exists
        /// </summary>
        public static bool DoesUserImageExist(int? userImageId)
        {
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var command = new SqlCommand("SP_DoesUserImageExist", connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@UserImageID", SqlDbType.Int).Value = userImageId;

                connection.Open();

                var result = command.ExecuteScalar();
                return result != null && result != DBNull.Value && (bool)result;
            }
            catch (Exception ex)
            {
                // Log exception
                return false;
            }
        }

        /// <summary>
        /// Gets all user images (admin use)
        /// </summary>
        public static DataTable GetAllUserImages()
        {
            var dt = new DataTable();

            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var command = new SqlCommand("SP_GetAllTbUserImages", connection);

                command.CommandType = CommandType.StoredProcedure;

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

        /// <summary>
        /// Gets all images for a specific user
        /// </summary>
        public static DataTable GetImagesByUserId(int userId)
        {
            var dt = new DataTable();

            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var command = new SqlCommand("SP_GetUserImagesByUserID", connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

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
