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
using PracticalApp.NorthwindMVC.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using PracticalApp.NorthwindContextLib;
using NorthwindMVC.Middlewares;
using StackExchange.Profiling.SqlFormatters;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace PracticalApp.NorthwindMVC
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
            // services.AddSession();
            var northwindDb = Path.Combine("..", "Northwind.db");
            services.AddDbContext<Northwind>(options => options.UseSqlite($"Data Source={northwindDb}"));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            // This is required only because we use HttpContext in the trace id message handler 
            services.AddHttpContextAccessor();
            // We should add Delegating Handlers as Transient. Otherwise we get this error:
            // The 'InnerHandler' property must be null. 'DelegatingHandler' instances provided to 'HttpMessageHandlerBuilder' must not be reused or cached (InvalidOperationException).
            services.AddTransient<TraceIdentifierMessageHandler>();
            // This is a microservice. We can register a HttpClient for each microservice
            // Add Delegating Handler here (They should be registered before hand)
            // Named Clients 
            services.AddHttpClient(name: "Northwind API", client =>
            {
                client.BaseAddress = new Uri("http://localhost:5000/");
                client.DefaultRequestHeaders.Accept
                .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json", 1.0));

            }).AddHttpMessageHandler<TraceIdentifierMessageHandler>(); // Message Handler is used to do some

            // Add Mini Profiler
            services.AddMiniProfiler(options =>
            {
                // options.RouteBasePath = "/profiler/";
                options.SqlFormatter = new VerboseSqlServerFormatter();
                options.StartProfiler();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            // Read body async for upload big files without blocking the request (It is async and use stream )
            // This is not required usually in MVC patterns since we Bind the parameters from body using [FromBody] attribute
            // But if you want to do something with body in middleware for example you can do it but to protect your app from performance and
            // security issue you should do it async and since Body is stream you should do with some type of Stream
            app.Use(next => async context =>
            {
                context.Request.EnableBuffering(); // This is necessary for using the multiple reading of Body.
                // Also we should reset the body position to zero to read from right position (I think seek does not work here and you should use position)
                // var qs = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery("?items=tea&items=cofee&count=3");
                // foreach (var item in qs)
                // {
                //     Console.WriteLine($"{item.Key}:{item.Value}");
                // }
                if (string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
                {
                    using (var reader = new HttpRequestStreamReader(context.Request.Body, Encoding.UTF8))
                    {
                        var body = await reader.ReadToEndAsync(); // This works but return string and it dont use newton
                        // var jsonReader = new JsonTextReader(reader); // this dont work and use newton
                        // var body = await JObject.LoadAsync(jsonReader);
                        Console.WriteLine(body);
                        context.Request.Body.Position = 0;
                    }
                }




                await next(context);
            });

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
            // app.UseMiniProfiler();// add a url with address /mini-profiler-resources shows performance metrics
            // There is a tag helper which we can add it to page and use it to view the metric related to that page
            // <mini-profile position=@RenderPosition.Right max-traces="5" color-scheme="@ColorScheme.Auto" />
            // to use it we should add this taghelper: @addTagHelper *, MiniProfiler.AspNetCore.Mvc
            app.UseHttpsRedirection();
            app.UseStaticFiles();



            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.Use(next => async context =>
            {
                if (string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
                {
                    using (var reader = new HttpRequestStreamReader(context.Request.Body, Encoding.UTF8))
                    {
                        //     var jsonReader = new JsonTextReader(reader);// this works only for json
                        //     var body = (await JObject.LoadAsync(jsonReader)).ToString();
                        var body = await reader.ReadToEndAsync();// This works for forms and Json as string
                        Console.WriteLine(body);
                        context.Request.Body.Position = 0;
                    }
                }
                await next(context);
            });


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
