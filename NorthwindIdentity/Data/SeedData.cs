using NorthwindIdentity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

// dotnet aspnet-codegenerator razorpage -m Contact -dc ApplicationDbContext -udl -outDir Pages\Contacts --referenceScriptLibraries

namespace NorthwindIdentity.Data
{
    public enum Roles {
        Admin,
        Manager
    }
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider services, string pass)
        {
            // Here we create new DbContext to reterieve db options like its name and path
            using (var context=new ApplicationDbContext(services.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                var adminId=await EnsureUser(services,"admin@contoso.com",pass);
                await EnsureRole(services,adminId,Roles.Admin.ToString());

                var managerId=await EnsureUser(services,"manager@contoso.com",pass);
                await EnsureRole(services,managerId,Roles.Manager.ToString());
                SeedDB(context,adminId,managerId);
            }
        }

        private static async Task<string> EnsureUser(IServiceProvider services,string username, string pass) {
            var userManager=services.GetRequiredService<UserManager<IdentityUser>>();
            var user=await userManager.FindByNameAsync(username);
            if (user==null) {
                user=new IdentityUser {
                    EmailConfirmed=true,
                    UserName=username
                };
                await userManager.CreateAsync(user,pass);
                if (user==null) {
                    throw new Exception("User Does not created (Possibly password is weak)");
                }
            }
            return user.Id;
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider services, string uid,string role) {
            var roleManager=services.GetRequiredService<RoleManager<IdentityRole>>();
            if (roleManager==null) {
                throw new Exception("roleManager null");
            }

            IdentityResult result;
            if (!(await roleManager.RoleExistsAsync(role))) {
                result=await roleManager.CreateAsync(new IdentityRole(role));
            }

            var userManager=services.GetRequiredService<UserManager<IdentityUser>>();
            var user=await userManager.FindByIdAsync(uid);
            if (user==null) {
                throw new Exception("User does not created already");
            }
            result=await userManager.AddToRoleAsync(user,role);
            return result;
        }

        public static void SeedDB(ApplicationDbContext context, string adminID,string managerId)
        {
            if (context.Contact.Any())
            {
                return;   // DB has been seeded
            }

            context.Contact.AddRange(
                new Contact
                {
                    Name = "Debra Garcia",
                    Address = "1234 Main St",
                    City = "Redmond",
                    State = "WA",
                    Zip = "10999",
                    Email = "debra@example.com",
                    OwnerID=adminID,
                    Status=ContactStatus.Approved

                },
                new Contact
                {
                    Name = "Thorsten Weinrich",
                    Address = "5678 1st Ave W",
                    City = "Redmond",
                    State = "WA",
                    Zip = "10999",
                    Email = "thorsten@example.com",
                    OwnerID=managerId,
                    Status=ContactStatus.Approved
                },
             new Contact
             {
                 Name = "Yuhong Li",
                 Address = "9012 State st",
                 City = "Redmond",
                 State = "WA",
                 Zip = "10999",
                 Email = "yuhong@example.com",
                OwnerID="3",
                Status=ContactStatus.Approved
             },
             new Contact
             {
                 Name = "Jon Orton",
                 Address = "3456 Maple St",
                 City = "Redmond",
                 State = "WA",
                 Zip = "10999",
                 Email = "jon@example.com",
                 OwnerID="4",
                Status=ContactStatus.Rejected
             },
             new Contact
             {
                 Name = "Diliana Alexieva-Bosseva",
                 Address = "7890 2nd Ave E",
                 City = "Redmond",
                 State = "WA",
                 Zip = "10999",
                 Email = "diliana@example.com",
                 OwnerID="5",
                Status=ContactStatus.Submited
             }
             );
            context.SaveChanges();
        }

    }
}