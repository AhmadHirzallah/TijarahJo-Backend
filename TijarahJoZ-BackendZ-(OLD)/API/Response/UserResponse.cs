using Domain;

namespace API.Response
{
	public class UserResponse
	{
		public int? PersonId;

		public string? UserName;

		public string? Email { get; set; }
		public string? FullName { get; set; }

		public string? Image { get; set; } // Multi Value Attribute 

		public string? Phone { get; set; }


		// JWT 
		public string? RefreshToken { get; set; }
		public DateTime? RefreshTokenExpiryTime { get; set; }



	}
	public static class UserReponseBinder
	{
		public static UserResponse Bind(UserModel user, Person person)
		{

			UserResponse userResponse = new UserResponse
			{
				Email = user.Email,
				UserName = user.UserName,
				FullName = person.FirstName + " " + person.LastName,
				Image = person.Image,
				Phone = person.Phone
			};

			return userResponse;
		}
	}

}
