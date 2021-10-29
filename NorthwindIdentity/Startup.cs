using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using NorthwindIdentity.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using NorthwindIdentity.AuthorizationHandler;

namespace NorthwindIdentity
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddRoles<IdentityRole>() // This line add role services to Identity
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            // The Output of Authentication:
            // Authentication may create one or more identities for the current user.
            services.AddAuthorization(options =>{
                // the fallback authentication policy requires all users to be authenticated,
                // except for Razor Pages, controllers, or action methods with an authentication attribute. 
                // options.FallbackPolicy=options.GetPolicy("PolicyName");
                //  Having authentication required by default is more secure than
                // relying on new controllers and Razor Pages to include the [Authorize] attribute.
                options.FallbackPolicy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                options.AddPolicy("admin",policy =>policy.RequireUserName("aaaa@gmail.com"));
                options.AddPolicy("local",builder =>{
                    builder.RequireAssertion(ctx => {
                        bool asserted=true;
                        if (ctx.Resource is AuthorizationFilterContext context) {
                            asserted=IPAddress.IsLoopback(context.HttpContext.Connection.RemoteIpAddress);
                        }
                        return asserted;
                    });
                });

                // The DefaultPolicy is the policy used with the [Authorize] attribute when no policy is specified.
                // [Authorize] doesn't contain a named policy, unlike [Authorize(PolicyName="MyPolicy")].
                // options.DefaultPolicy=options.GetPolicy("local");

                // We can use requirement directly in AuthorizeAsync or using a Policy as follow
                options.AddPolicy("IsMonday",builder =>builder.AddRequirements(new DayOfWeekAuthorizationRequirement(DayOfWeek.Monday)));

            });
            services.AddSingleton<IAuthorizationHandler,DayOfWeekAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler,SameAuthorAuthorizationHandler>();

            // We should register it as Scoped since it uses HttpContext
            services.AddScoped<IAuthorizationHandler,LocalIPAuthorizationHandler>();

            // This uses Entity Framework Core so we should use it scoped since:
            // Services using Entity Framework Core must be registered for dependency injection using AddScoped.
            services.AddScoped<IAuthorizationHandler,ContactIsResourceOwner>();
            services.AddSingleton<IAuthorizationHandler,ContactIsManager>();

            //  singletons because they don't use EF and all the information needed is in the Context parameter
            // of the HandleRequirementAsync method
            services.AddSingleton<IAuthorizationHandler,ContactIsAdminsitrator>();

        
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
