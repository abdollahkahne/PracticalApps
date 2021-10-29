using System;

namespace NorthwindCookieAuth.Data
{
    public class ApplicationUser
    {
        public string Email { get; set; }
        public string FullName {get;set;}
        public DateTime Modified { get; set; }
        public string Country {get;set;}
        public int Age {get;set;}
    }
}