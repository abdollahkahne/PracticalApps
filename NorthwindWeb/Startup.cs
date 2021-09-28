using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using Microsoft.EntityFrameworkCore;
using PracticalApp.NorthwindContextLib;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace PracticalApp.NorthwindWeb
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            // // How to get an instance of service manually
            // var serviceProvider=services.BuildServiceProvider();
            // var db=serviceProvider.GetService<Northwind>();

            string dbPath = Path.Combine("..", "Northwind.db");
            services.AddDbContext<Northwind>(options => options.UseSqlite($"Data Source={dbPath}"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,IHostApplicationLifetime events)
        {
            //Run in the application start only:
            events.ApplicationStarted.Register(()=>{
                var feature=app.ServerFeatures.Get<IServerAddressesFeature>();
                // Features are found in Microsoft.AspNetCore.Http namespace 
                // and in the Microsoft.AspNetCore.Hosting.Server namespace.
                //But all of this features implemented in httpcontext instance
                var addresses=feature.Addresses;
                foreach (var item in addresses)
                {
                    Console.WriteLine(@"Address{0}",item);
                }
            });


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseRouting();
            app.UseHttpsRedirection();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                // endpoints.MapGet("/test", async context =>
                // {
                //     await context.Response.WriteAsync("Hello World!");
                // });
            });
        }
    }
}
