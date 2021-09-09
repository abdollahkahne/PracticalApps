using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using PracticalApp.NorthwindContextLib;
using PracticalApp.NorthwindAPI.Middlewares;
using System.IO;
using Microsoft.AspNetCore.Mvc.Formatters;
using PracticalApp.NorthwindAPI.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace PracticalApp.NorthwindAPI
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
            services.AddCors();
            var dbPath = Path.Combine("..", "Northwind.db");
            services.AddDbContext<Northwind>(options => options.UseSqlite($"Data Source={dbPath}"));

            // services.AddControllers(option =>
            // {
            //     Console.WriteLine("Default Output Formatters are:");
            //     foreach (var item in option.OutputFormatters)
            //     {
            //         var mediaFormatter = item as OutputFormatter;
            //         if (mediaFormatter == null)
            //         {
            //             Console.WriteLine(item.ToString());
            //         }
            //         else
            //         {
            //             Console.WriteLine($"{mediaFormatter.ToString()}:{String.Join(',', mediaFormatter.SupportedMediaTypes)}");
            //         }

            //     }
            // })
            services.AddControllers()
            .AddXmlDataContractSerializerFormatters()
            .AddXmlSerializerFormatters()
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NorthwindAPI", Version = "v1" });
            });

            services.AddHealthChecks().AddDbContextCheck<Northwind>();

            services.AddScoped<ICustomerRepository, CustomerRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NorthwindAPI v1"));
                app.UseHealthChecks(path: "/health-check");
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors(options =>
            {
                options.WithMethods("GET", "POST", "DELETE", "PUT");
                options.WithOrigins("https://localhost:5002");
            });

            app.UseAuthorization();

            app.Use(next => async (context) =>
            {
                Console.WriteLine("First Custome Middleware On Request");
                await next(context);
                Console.WriteLine("Last Custome Middleware On Response");
            });

            app.Use(next => (context) =>
            {
                Console.WriteLine("Second Custome Middleware On Request");
                var endpoint = context.GetEndpoint();
                var routeData = context.GetRouteData();
                if (routeData != null)
                {
                    foreach (var item in routeData.Values)
                    {
                        Console.WriteLine($"{item.Key}:{item.Value.ToString()}");
                    }

                }
                // if (endpoint != null)
                // {
                //     Console.WriteLine($"Name:{endpoint.DisplayName};Route: {(endpoint as RouteEndpoint)?.RoutePattern}; Metadata: {String.Join(", ", endpoint.Metadata)}");
                // }

                return next(context);
            });

            app.Use(next => async (context) =>
            {
                Console.WriteLine("Third Custome Middleware on Request");
                await next(context);
                Console.WriteLine("First Custome Middleware on Response");
            });

            app.UseLogNavigationData();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
