using Domain;
using Infrastructure;
using System.Reflection.Metadata.Ecma335;


namespace Services.PersonServices
{
    public class PersonService
    {
        
        public PersonService()
        {
        }

        // We get Request
        public Person GetPerson(User user)
        {
            UserData userData = new UserData();

            user = userData.VerifyUser(user.UserName, user.Password);

            
            // Get from Database
            return  (new Person
            {
                FirstName = "John",
                LastName = "Doe",
            });
        }
    }
}
