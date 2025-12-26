using System;
using System.Data;
using System.Collections.Generic;
using TijarahJoDB.DAL;
using Models;

namespace TijarahJoDB.BLL
{
    /// <summary>
    /// Business Logic Layer for User Phone Numbers
    /// One-to-Many relationship: One User can have many Phone Numbers
    /// </summary>
    public class UserPhoneNumberBL
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode = enMode.AddNew;

        public UserPhoneNumberModel UserPhoneNumberModel =>
            new UserPhoneNumberModel(PhoneID, UserID, PhoneNumber, IsPrimary, CreatedAt, IsDeleted);

        public int? PhoneID { get; set; }
        public int UserID { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public UserPhoneNumberBL(UserPhoneNumberModel model, enMode mode = enMode.AddNew)
        {
            PhoneID = model.PhoneID;
            UserID = model.UserID;
            PhoneNumber = model.PhoneNumber;
            IsPrimary = model.IsPrimary;
            CreatedAt = model.CreatedAt;
            IsDeleted = model.IsDeleted;
            Mode = mode;
        }

        /// <summary>
        /// Finds a phone number by ID
        /// </summary>
        public static UserPhoneNumberBL? Find(int phoneId)
        {
            var model = UserPhoneNumberData.GetByID(phoneId);

            if (model != null)
                return new UserPhoneNumberBL(model, enMode.Update);

            return null;
        }

        /// <summary>
        /// Gets all phone numbers for a specific user
        /// </summary>
        public static List<UserPhoneNumberBL> GetByUserID(int userId)
        {
            var list = new List<UserPhoneNumberBL>();
            var dt = UserPhoneNumberData.GetByUserID(userId);

            foreach (DataRow row in dt.Rows)
            {
                var model = new UserPhoneNumberModel(
                    (int)row["PhoneID"],
                    (int)row["UserID"],
                    (string)row["PhoneNumber"],
                    (bool)row["IsPrimary"],
                    (DateTime)row["CreatedAt"],
                    (bool)row["IsDeleted"]
                );
                list.Add(new UserPhoneNumberBL(model, enMode.Update));
            }

            return list;
        }

        /// <summary>
        /// Gets all phone numbers for a user as DataTable
        /// </summary>
        public static DataTable GetAllByUserID(int userId) =>
            UserPhoneNumberData.GetByUserID(userId);

        private bool _AddNew()
        {
            PhoneID = UserPhoneNumberData.Add(UserPhoneNumberModel);
            return PhoneID != -1;
        }

        private bool _Update()
        {
            return UserPhoneNumberData.Update(UserPhoneNumberModel);
        }

        /// <summary>
        /// Saves the phone number (Insert or Update based on Mode)
        /// </summary>
        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNew())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _Update();
            }
            return false;
        }

        /// <summary>
        /// Deletes a phone number
        /// </summary>
        public static bool Delete(int phoneId) =>
            UserPhoneNumberData.Delete(phoneId);

        /// <summary>
        /// Checks if a phone number exists
        /// </summary>
        public static bool Exists(int phoneId) =>
            UserPhoneNumberData.Exists(phoneId);
    }
}
