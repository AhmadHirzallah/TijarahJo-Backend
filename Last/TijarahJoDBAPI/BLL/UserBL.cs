using System;
using System.Data;
using TijarahJoDB.DAL;
using Models;

namespace TijarahJoDB.BLL
{
	public class UserBL
	{
		public enum enMode { AddNew = 0, Update = 1 };
		public enMode Mode = enMode.AddNew;

		public UserModel UserModel
		{
			get { return new UserModel(UserID = this.UserID, Username = this.Username, HashedPassword = this.HashedPassword, Email = this.Email, FirstName = this.FirstName, LastName = this.LastName, JoinDate = this.JoinDate, Status = this.Status, RoleID = this.RoleID, IsDeleted = this.IsDeleted); }
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

		public UserBL(UserModel UserModel, enMode cMode = enMode.AddNew)
		{
			this.UserID = UserModel.UserID;
			this.Username = UserModel.Username;
			this.HashedPassword = UserModel.HashedPassword;
			this.Email = UserModel.Email;
			this.FirstName = UserModel.FirstName;
			this.LastName = UserModel.LastName;
			this.JoinDate = UserModel.JoinDate;
			this.Status = UserModel.Status;
			this.RoleID = UserModel.RoleID;
			this.IsDeleted = UserModel.IsDeleted;
			Mode = cMode;
		}

		private bool _AddNewUser()
		{
			this.UserID = (int?)UserData.AddUser(UserModel);
			return (this.UserID != -1);
		}

		private bool _UpdateUser()
		{
			return UserData.UpdateUser(UserModel);
		}

		public static UserBL Find(int? UserID)
		{
			UserModel UserModel = UserData.GetUserByID(UserID);

			if (UserModel != null)
				return new UserBL(UserModel, enMode.Update);
			else
				return null;
		}

		public bool Save()
		{
			switch (Mode)
			{
				case enMode.AddNew:
					if (_AddNewUser())
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
