using System;
using System.Data;
using TijarahJoDB.DAL;
using Models;

namespace TijarahJoDB.BLL
{
    /// <summary>
    /// Business logic layer for User Image operations
    /// </summary>
    public class UserImage
    {
        public enum enMode { AddNew = 0, Update = 1 }

        public enMode Mode { get; private set; } = enMode.AddNew;

        #region Properties

        public int? UserImageID { get; set; }
        public int UserID { get; set; }
        public string ImageURL { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public bool IsDeleted { get; set; }

        #endregion

        #region Model Conversion

        public UserImageModel UserImageModel => new UserImageModel(
            UserImageID,
            UserID,
            ImageURL,
            UploadedAt,
            IsDeleted
        );

        #endregion

        #region Constructors

        public UserImage(UserImageModel model, enMode mode = enMode.AddNew)
        {
            UserImageID = model.UserImageID;
            UserID = model.UserID;
            ImageURL = model.ImageURL;
            UploadedAt = model.UploadedAt;
            IsDeleted = model.IsDeleted;
            Mode = mode;
        }

        #endregion

        #region Private Methods

        private bool _AddNewUserImage()
        {
            UserImageID = UserImageData.AddUserImage(UserImageModel);
            return UserImageID != -1;
        }

        private bool _UpdateUserImage()
        {
            return UserImageData.UpdateUserImage(UserImageModel);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Saves the user image (insert or update based on mode)
        /// </summary>
        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewUserImage())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _UpdateUserImage();

                default:
                    return false;
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Finds a user image by ID
        /// </summary>
        public static UserImage? Find(int? userImageId)
        {
            var model = UserImageData.GetUserImageByID(userImageId);
            return model != null ? new UserImage(model, enMode.Update) : null;
        }

        /// <summary>
        /// Deletes a user image by ID
        /// </summary>
        public static bool DeleteUserImage(int? userImageId)
            => UserImageData.DeleteUserImage(userImageId);

        /// <summary>
        /// Checks if a user image exists
        /// </summary>
        public static bool DoesUserImageExist(int? userImageId)
            => UserImageData.DoesUserImageExist(userImageId);

        /// <summary>
        /// Gets all user images
        /// </summary>
        public static DataTable GetAllUserImages()
            => UserImageData.GetAllUserImages();

        /// <summary>
        /// Gets all images for a specific user
        /// </summary>
        public static DataTable GetImagesByUserId(int userId)
            => UserImageData.GetImagesByUserId(userId);

        #endregion
    }
}
