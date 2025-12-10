using System;

{
    public class UserModel
    {
        public UserModel(int? userid, string username, string hashedpassword, string email, string firstname, string lastname, DateTime joindate, int status, int roleid, bool isdeleted)
        {
            this.UserID = userid;
            this.Username = username;
            this.HashedPassword = hashedpassword;
            this.Email = email;
            this.FirstName = firstname;
            this.LastName = lastname;
            this.JoinDate = joindate;
            this.Status = status;
            this.RoleID = roleid;
            this.IsDeleted = isdeleted;
        }

        public int? UserID { get; set; }
        public string Username { get; set; }
        public string HashedPassword { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime JoinDate { get; set; }
        public int Status { get; set; }
        public int RoleID { get; set; }
        public bool IsDeleted { get; set; }
    }
}
