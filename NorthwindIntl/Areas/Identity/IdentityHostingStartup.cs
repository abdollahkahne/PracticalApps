using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NorthwindIntl.Areas.Identity.Data;

[assembly: HostingStartup(typeof(NorthwindIntl.Areas.Identity.IdentityHostingStartup))]
namespace NorthwindIntl.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            // builder.ConfigureServices((context, services) => {
            //     services.AddDbContext<NorthwindIntlIdentityDbContext>(options =>
            //         options.UseSqlite(
            //             context.Configuration.GetConnectionString("NorthwindIntlIdentityDbContextConnection")));

            //     services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //         .AddEntityFrameworkStores<NorthwindIntlIdentityDbContext>();
            // });
        }
    }
}