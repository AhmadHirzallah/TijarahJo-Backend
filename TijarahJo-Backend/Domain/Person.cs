using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public enum PersonStatus
    {
        Active,
        InActive,
        Banned
    }

    public class Person
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // 1 Person => Many Images ====== New Table for Images
        public string? Image { get; set; } // Multi Value Attribute 

        // 1 Person => Many Phone ====== New Table for Images
        public string? Phone { get; set; }

        // Psudue Delete -- Make Inactive/Blocked/Active       
        public PersonStatus personStatus { get; set; }

        // JWT 
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
