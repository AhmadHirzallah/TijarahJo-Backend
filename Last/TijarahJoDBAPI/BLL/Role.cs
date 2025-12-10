using System;
using System.Data;
using Models;
using TijarahJoDB.DAL;

namespace TijarahJoDB.BLL
{
	public class Role
	{
		public enum enMode { AddNew = 0, Update = 1 };
		public enMode Mode = enMode.AddNew;

		public RoleModel RoleModel
		{
			get { return new RoleModel(RoleID = this.RoleID, RoleName = this.RoleName, CreatedAt = this.CreatedAt, IsDeleted = this.IsDeleted); }
		}

		public int? RoleID { set; get; }
		public string RoleName { set; get; }
		public DateTime CreatedAt { set; get; }
		public bool IsDeleted { set; get; }

		public Role(RoleModel RoleModel, enMode cMode = enMode.AddNew)
		{
			this.RoleID = RoleModel.RoleID;
			this.RoleName = RoleModel.RoleName;
			this.CreatedAt = RoleModel.CreatedAt;
			this.IsDeleted = RoleModel.IsDeleted;
			Mode = cMode;
		}

		private bool _AddNewRole()
		{
			this.RoleID = (int?)RoleData.AddRole(RoleModel);
			return (this.RoleID != -1);
		}

		private bool _UpdateRole()
		{
			return RoleData.UpdateRole(RoleModel);
		}

		public static Role Find(int? RoleID)
		{
			RoleModel RoleModel = RoleData.GetRoleByID(RoleID);

			if (RoleModel != null)
				return new Role(RoleModel, enMode.Update);
			else
				return null;
		}

		public bool Save()
		{
			switch (Mode)
			{
				case enMode.AddNew:
					if (_AddNewRole())
					{
						Mode = enMode.Update;
						return true;
					}
					else
					{
						return false;
					}

				case enMode.Update:
					return _UpdateRole();
			}
			return false;
		}
		public static bool DeleteRole(int? RoleID)
			=> RoleData.DeleteRole(RoleID);
		public static bool DoesRoleExist(int? RoleID)
			=> RoleData.DoesRoleExist(RoleID);
		public static DataTable GetAllTbRoles()
			=> RoleData.GetAllTbRoles();
	}
}
