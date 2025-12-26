using System;

namespace Models;

public class UserModel
{
    public UserModel(int? userid,
                     string username,
                     string hashedpassword,
                     string email,
                     string firstname,
                     string? lastname,
                     DateTime joindate,
                     int status,
                     int roleid,
                     bool isdeleted,
                     string? primaryPhone = null)
    {
        UserID = userid;
        Username = username;
        HashedPassword = hashedpassword;
        Email = email;
        FirstName = firstname;
        LastName = lastname;
        JoinDate = joindate;
        Status = status;
        RoleID = roleid;
        IsDeleted = isdeleted;
        PrimaryPhone = primaryPhone;
    }

    public int? UserID { get; set; }
    public string Username { get; set; }
    public string HashedPassword { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime JoinDate { get; set; }
    public int Status { get; set; }
    public int RoleID { get; set; }
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Primary phone number (read from TbUserPhoneNumbers)
    /// </summary>
    public string? PrimaryPhone { get; set; }
}
