using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NorthwindIntl.Areas.Identity.Data
{
    public class ApplicationUser:IdentityUser
    {
        public ApplicationUser() {}
        public ApplicationUser(string userName):base(userName) {}
        [PersonalData]
        [MaxLength(50,ErrorMessage =@"You should use maximum {1} chars for {0}")]
        public string FullName { get; set; }

        [PersonalData]
        public DateTime? DoB { get; set; }
    }
}