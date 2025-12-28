using System;
using System.Data;
using TijarahJoDB.DAL;
using Models;

namespace TijarahJoDB.BLL
{
    /// <summary>
    /// Business logic layer for Review operations
    /// </summary>
    public class Review
    {
        public enum enMode { AddNew = 0, Update = 1 }
        
        public enMode Mode { get; private set; } = enMode.AddNew;

        #region Properties

        public int? ReviewID { get; set; }
        public int PostID { get; set; }
        public int UserID { get; set; }
        public byte Rating { get; set; }
        public string? ReviewText { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }

        #endregion

        #region Model Conversion

        public ReviewModel ReviewModel => new ReviewModel(
            ReviewID,
            PostID,
            UserID,
            Rating,
            ReviewText,
            CreatedAt,
            IsDeleted
        );

        #endregion

        #region Constructors

        public Review(ReviewModel model, enMode mode = enMode.AddNew)
        {
            ReviewID = model.ReviewID;
            PostID = model.PostID;
            UserID = model.UserID;
            Rating = model.Rating;
            ReviewText = model.ReviewText;
            CreatedAt = model.CreatedAt;
            IsDeleted = model.IsDeleted;
            Mode = mode;
        }

        #endregion

        #region Private Methods

        private bool _AddNewReview()
        {
            ReviewID = ReviewData.AddReview(ReviewModel);
            return ReviewID != -1;
        }

        private bool _UpdateReview()
        {
            return ReviewData.UpdateReview(ReviewModel);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Saves the review (insert or update based on mode)
        /// </summary>
        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewReview())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _UpdateReview();

                default:
                    return false;
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Finds a review by ID
        /// </summary>
        public static Review? Find(int? reviewId)
        {
            var model = ReviewData.GetReviewByID(reviewId);
            return model != null ? new Review(model, enMode.Update) : null;
        }

        /// <summary>
        /// Deletes a review by ID
        /// </summary>
        public static bool DeleteReview(int? reviewId)
            => ReviewData.DeleteReview(reviewId);

        /// <summary>
        /// Checks if a review exists
        /// </summary>
        public static bool DoesReviewExist(int? reviewId)
            => ReviewData.DoesReviewExist(reviewId);

        /// <summary>
        /// Gets all reviews
        /// </summary>
        public static DataTable GetAllReviews()
            => ReviewData.GetAllReviews();

        /// <summary>
        /// Gets all reviews for a specific post
        /// </summary>
        public static DataTable GetReviewsByPostId(int postId)
            => ReviewData.GetReviewsByPostId(postId);

        /// <summary>
        /// Soft deletes all reviews for a specific post
        /// </summary>
        /// <param name="postId">The post ID</param>
        /// <returns>Number of reviews deleted, -1 on error</returns>
        public static int SoftDeleteByPostId(int postId)
            => ReviewData.SoftDeleteReviewsByPostId(postId);

        #endregion
    }
}
