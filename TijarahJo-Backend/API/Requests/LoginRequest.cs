using Domain;
using System.ComponentModel.DataAnnotations;

namespace API.Requests
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "UserName is required.")]
        public string UserName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;
    }


    // Binding Between LoginRequest && User Domain Model
    public static class LoginRequestBinder
    {
        public static User Bind(LoginRequest loginRequest)
        {
            return new User
            {
                Email = loginRequest.Email,
                UserName = loginRequest.UserName,
                Password = loginRequest.Password
            };
        }
    }

}
