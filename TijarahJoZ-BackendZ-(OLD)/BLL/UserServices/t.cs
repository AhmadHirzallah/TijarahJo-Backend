using System;
using System.Data;
using TijarahJoDB.Models;
using TijarahJoDB.DAL;

namespace TijarahJoDB.BLL
{
    public class User
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode = enMode.AddNew;

        public UserDTO UserDTO
        {
            get { return new UserDTO(UserID = this.UserID, Username = this.Username, HashedPassword = this.HashedPassword, Email = this.Email, FirstName = this.FirstName, LastName = this.LastName, JoinDate = this.JoinDate, Status = this.Status, RoleID = this.RoleID, IsDeleted = this.IsDeleted); }
        }

        public int? UserID { set; get; }
        public string Username { set; get; }
        public string HashedPassword { set; get; }
        public string Email { set; get; }
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public DateTime JoinDate { set; get; }
        public int Status { set; get; }
        public int RoleID { set; get; }
        public bool IsDeleted { set; get; }

        public User(UserDTO UserDTO, enMode cMode = enMode.AddNew)
        {
            this.UserID = UserDTO.UserID;
            this.Username = UserDTO.Username;
            this.HashedPassword = UserDTO.HashedPassword;
            this.Email = UserDTO.Email;
            this.FirstName = UserDTO.FirstName;
            this.LastName = UserDTO.LastName;
            this.JoinDate = UserDTO.JoinDate;
            this.Status = UserDTO.Status;
            this.RoleID = UserDTO.RoleID;
            this.IsDeleted = UserDTO.IsDeleted;
            Mode = cMode;
        }

        private bool _AddNewUser()
        {
            this.UserID = (int?)UserData.AddUser(UserDTO);
            return (this.UserID != -1);
        }

        private bool _UpdateUser()
        {
            return UserData.UpdateUser(UserDTO);
        }

        public static User Find(int? UserID)
        {
            UserDTO UserDTO = UserData.GetUserByID(UserID);

            if(UserDTO != null)
                return new User(UserDTO, enMode.Update);
            else
                return null;
        }

        public bool Save()
        {
            switch(Mode)
            {
                case enMode.AddNew:
                    if(_AddNewUser())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case enMode.Update:
                    return _UpdateUser();
            }
            return false;
        }
        public static bool DeleteUser(int? UserID)
            => UserData.DeleteUser(UserID);
        public static bool DoesUserExist(int? UserID)
            => UserData.DoesUserExist(UserID);
        public static DataTable GetAllTbUsers()
            => UserData.GetAllTbUsers();
    }
}
