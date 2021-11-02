using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControllersTestSample.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Src.Core.Interfaces;
using Src.Core.Models;
using Src.Features;
using Src.Infrastructure;

namespace Src
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("InMemory"));
            services.AddScoped<IBrainstormSessionRepository, EfStormSessionRepository>();
            services.AddControllersWithViews();

            // since our clock Middleware uses IMiddleware interface we should register it in services 
            // this is necessary because we are using the IMiddleware interface and the Middleware factory
            services.AddScoped<ClockMiddleware>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {

            app.Use(next => async (ctx) =>
              {
                  ClockMiddleware.SetTimeZone(ctx.Response, "Asia/Tehran");
                  await next(ctx);
              });

            app.UseMiddleware<ClockMiddleware>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // In this way we do some Initialization on each request!! which is better to move it to Program.cs since service provider is availabe after building host and before running it
                // Here we do not use HttpContext to get the service but we used service provider directly and we pass it to 
                // function that generate a new Brainstorm session which has one Idea too
                var repository = serviceProvider.GetRequiredService<IBrainstormSessionRepository>();
                InitializeDatabaseAsync(repository).Wait();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseGlobalHeader();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public async Task InitializeDatabaseAsync(IBrainstormSessionRepository repository)
        {
            var sessionList = await repository.ListAsync();
            if (!sessionList.Any())
            {
                await repository.AddAsync(GetTestSession());
            }
        }
        public static BrainstormSession GetTestSession()
        {
            var session = new BrainstormSession
            {
                Name = "Test Session 1",
                DateCreated = new DateTime(2016, 8, 1),
            };

            var idea = new Idea
            {
                Name = "Awesome idea",
                Description = "Totally awsome idea",
                DateCreated = new DateTime(2016, 8, 1)
            };
            session.Ideas.Add(idea);
            return session;
        }
    }
}
