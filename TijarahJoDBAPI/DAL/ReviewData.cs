using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Models;
using TijarahJoDB_DataAccess;

namespace TijarahJoDB.DAL
{
    public class ReviewData
    {
        /// <summary>
        /// Gets a review by its ID
        /// </summary>
        public static ReviewModel? GetReviewByID(int? reviewId)
        {
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var command = new SqlCommand("SP_GetReviewByID", connection);
                
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@ReviewID", SqlDbType.Int).Value = reviewId;

                connection.Open();

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new ReviewModel(
                        reader.GetInt32(reader.GetOrdinal("ReviewID")),
                        reader.GetInt32(reader.GetOrdinal("PostID")),
                        reader.GetInt32(reader.GetOrdinal("UserID")),
                        reader.GetByte(reader.GetOrdinal("Rating")),
                        reader.IsDBNull(reader.GetOrdinal("ReviewText")) 
                            ? null 
                            : reader.GetString(reader.GetOrdinal("ReviewText")),
                        reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
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
        /// Adds a new review
        /// </summary>
        public static int AddReview(ReviewModel model)
        {
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var command = new SqlCommand("SP_AddReview", connection);
                
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@PostID", SqlDbType.Int).Value = model.PostID;
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = model.UserID;
                command.Parameters.Add("@Rating", SqlDbType.TinyInt).Value = model.Rating;
                command.Parameters.Add("@ReviewText", SqlDbType.NVarChar, 1000).Value = 
                    (object?)model.ReviewText ?? DBNull.Value;
                command.Parameters.Add("@CreatedAt", SqlDbType.DateTime).Value = model.CreatedAt;
                command.Parameters.Add("@IsDeleted", SqlDbType.Bit).Value = model.IsDeleted;

                var outputIdParam = new SqlParameter("@NewReviewID", SqlDbType.Int)
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
        /// Updates an existing review
        /// </summary>
        public static bool UpdateReview(ReviewModel model)
        {
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var command = new SqlCommand("SP_UpdateReview", connection);
                
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@ReviewID", SqlDbType.Int).Value = model.ReviewID;
                command.Parameters.Add("@PostID", SqlDbType.Int).Value = model.PostID;
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = model.UserID;
                command.Parameters.Add("@Rating", SqlDbType.TinyInt).Value = model.Rating;
                command.Parameters.Add("@ReviewText", SqlDbType.NVarChar, 1000).Value = 
                    (object?)model.ReviewText ?? DBNull.Value;
                command.Parameters.Add("@CreatedAt", SqlDbType.DateTime).Value = model.CreatedAt;
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
        /// Soft deletes a review
        /// </summary>
        public static bool DeleteReview(int? reviewId)
        {
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var command = new SqlCommand("SP_DeleteReview", connection);
                
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@ReviewID", SqlDbType.Int).Value = reviewId;

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
        /// Checks if a review exists
        /// </summary>
        public static bool DoesReviewExist(int? reviewId)
        {
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var command = new SqlCommand("SP_DoesReviewExist", connection);
                
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@ReviewID", SqlDbType.Int).Value = reviewId;

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
        /// Gets all reviews
        /// </summary>
        public static DataTable GetAllReviews()
        {
            var dt = new DataTable();

            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var command = new SqlCommand("SP_GetAllTbPostReviews", connection);
                
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
        /// Gets all reviews for a specific post
        /// </summary>
        public static DataTable GetReviewsByPostId(int postId)
        {
            var dt = new DataTable();

            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var command = new SqlCommand("SP_GetReviewsByPostID", connection);
                
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
