using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Src.Data.IdentityModels
{
    public enum Country
    {
        UK = 1,
        US,
        UAE,
        Norway,
        France,
        Germany,
        Russia
    }
    public class AppUser : IdentityUser
    {
        // We have by default the following Properties and this class only use if we want to extend the user fields
        // Id,UserName, Claims,Roles,Email,PhoneNumber, SecurityStamp, Hashed Password
        public Country Country { get; set; }
        public int Age { get; set; }
        [Required]
        public string Salary { get; set; }

    }
    public class AppIdentityDbContext : IdentityDbContext<AppUser>
    {
        // Here we can define the context used for Identity, The default uses Identity User and so it easily can be inherited from IdentityDbContext to add methods or models to its context
        // Like other db context the constructor with option should be defined 
        public AppIdentityDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}