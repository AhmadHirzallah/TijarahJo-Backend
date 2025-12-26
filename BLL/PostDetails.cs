using TijarahJoDB.DAL;

namespace TijarahJoDB.BLL
{
    /// <summary>
    /// Business logic for getting complete post details
    /// </summary>
    public static class PostDetails
    {
        /// <summary>
        /// Gets complete post details including owner info, reviews, and images
        /// </summary>
        /// <param name="postId">The post ID</param>
        /// <returns>PostDetailsDataResult containing all data</returns>
        public static PostDetailsDataResult GetPostDetailsAll(int postId)
            => PostDetailsData.GetPostDetailsAll(postId);
    }
}
