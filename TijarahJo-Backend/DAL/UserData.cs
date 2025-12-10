using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class UserData
    {
        public User VerifyUser(string userName, string password)
        {
            
            if (userName == "MockUser" && password == "MockPassword")
            {
                return new User()
                { 
                    UserName = userName,
                    Email = "aasdasd@gmail.com",
                    
                };
            }

            return null;
        }

    }
}
