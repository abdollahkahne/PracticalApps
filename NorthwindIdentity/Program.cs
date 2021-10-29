using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NorthwindIdentity.Data;

namespace NorthwindIdentity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // CreateHostBuilder(args).Build().Run();
            // In Order to do stuff in build time we can write as follow:
            var host=CreateHostBuilder(args).Build();
            using (var scope=host.Services.CreateScope())
            {
                 var services=scope.ServiceProvider;
                 try
                 {
                     // Do every migration which not applied to db
                      var dbContext=services.GetRequiredService<ApplicationDbContext>();
                      dbContext.Database.Migrate();

                      // Initialize Seed Data
                      var config=services.GetRequiredService<IConfiguration>();
                      var seedDataPWD=config["SeedUserPW"];

                      SeedData.Initialize(services,"P@s$w0rd").Wait(); // Wait do same as await in non-async methods
                 }
                 catch (System.Exception ex)
                 {
                     
                     var logger=services.GetRequiredService<ILogger<Program>>();
                     logger.LogError(ex,"An error occured while seeding database");
                 }
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
