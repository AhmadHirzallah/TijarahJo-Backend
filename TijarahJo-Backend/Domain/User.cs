using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class User
    {
        public int? PersonId;

        public string? UserName;


        // 1 Person => Many Emails ====== For recovery
        public string? Email { get; set; }

        // This password will be hashed password
        public string? Password;


        // FK
        public int UserTypeId { get; set; } // Customer , VIP1, VIP2

    }
}
