using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAdminBot.Models
{
    public class UserProfile
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public bool UserAuthenticated { get; set; } = false;
        public int OTP { get; set; }
        public bool attendance { get; set; }
        public string storeName { get; set; }
        public string Message { get; set; }
        public string location { get; set; }
        public bool EmailVerified { get; set; } = false;
        public string ValueFinder { get; set; }
    }
}
