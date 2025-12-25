using System;
using System.Data;
using Microsoft.Data.SqlClient;
using TijarahJoDB_DataAccess;

namespace TijarahJoDB.DAL
{
    /// <summary>
    /// Data result from SP_GetPostDetails_All containing post details, reviews, and images
    /// </summary>
    public class PostDetailsDataResult
    {
        public DataTable PostDetails { get; set; } = new();
        public DataTable Reviews { get; set; } = new();
        public DataTable Images { get; set; } = new();
        public bool HasData => PostDetails.Rows.Count > 0;
    }

    /// <summary>
    /// Data access for getting complete post details including reviews and images
    /// </summary>
    public class PostDetailsData
    {
        /// <summary>
        /// Gets complete post details including owner info, reviews, and images
        /// Uses SP_GetPostDetails_All which returns 3 result sets
        /// </summary>
        /// <param name="postId">The post ID</param>
        /// <returns>PostDetailsDataResult containing all data or empty if not found</returns>
        public static PostDetailsDataResult GetPostDetailsAll(int postId)
        {
            var result = new PostDetailsDataResult();

            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var command = new SqlCommand("SP_GetPostDetails_All", connection);
                
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@PostID", SqlDbType.Int).Value = postId;

                // Use DataSet with SqlDataAdapter to properly handle multiple result sets
                using var adapter = new SqlDataAdapter(command);
                var dataSet = new DataSet();
                
                connection.Open();
                adapter.Fill(dataSet);

                // Result Set 0: Post + Owner + Category + Role (ONE row)
                if (dataSet.Tables.Count > 0)
                {
                    result.PostDetails = dataSet.Tables[0];
                }

                // Result Set 1: Reviews (MANY rows, may be empty)
                if (dataSet.Tables.Count > 1)
                {
                    result.Reviews = dataSet.Tables[1];
                }

                // Result Set 2: Images (MANY rows, may be empty)
                if (dataSet.Tables.Count > 2)
                {
                    result.Images = dataSet.Tables[2];
                }

#if DEBUG
                // Debug: Log what columns we received
                for (int t = 0; t < dataSet.Tables.Count; t++)
                {
                    var cols = string.Join(", ", dataSet.Tables[t].Columns
                        .Cast<DataColumn>()
                        .Select(c => c.ColumnName));
                    System.Diagnostics.Debug.WriteLine($"[PostDetailsData] Table[{t}] ({dataSet.Tables[t].Rows.Count} rows): {cols}");
                }
#endif
            }
            catch (Exception ex)
            {
                // Log exception
                System.Diagnostics.Debug.WriteLine($"[PostDetailsData] Error: {ex.Message}");
            }

            return result;
        }
    }
}
