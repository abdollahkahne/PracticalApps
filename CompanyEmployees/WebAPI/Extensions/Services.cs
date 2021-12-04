using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.IISIntegration;
using Contracts;
using LoggerService;
using Microsoft.Extensions.Configuration;
using Entities;
using Microsoft.EntityFrameworkCore;
using Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using WebAPI.Formatters;

namespace WebAPI.Extensions
{
    public static class Services
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policyBuilder =>
                {
                    // policyBuilder.WithOrigins("https://example.com", "https://sample.com")
                    // .WithMethods("POST", "GET")
                    // .WithHeaders("accept", "content-type", "authorization");

                    policyBuilder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            });
        }

        public static void ConfigureIISIntegration(this IServiceCollection services)
        {
            // This has following defaults"
            // AuthenticationDisplayName=null
            // AutomaticAuthentication=true
            // ForwardClientCertificate=false
            services.Configure<IISOptions>(options =>
            {
                // options.AuthenticationDisplayName="Login Page Display";// This is showed in login page to users
                // options.AutomaticAuthentication = true; // If this is true, we should enable windows user authentication on IIS and Authentication Middleware response to all challange and sets HttpContext.User
                // Otherwise it only sets HttpContext.User and only respond to challanges when it enabled explicitly by Authentication schema 
                // options.ForwardClientCertificate=true; // If MS-ASPNETCORE-CLIENTCERT request header exist populate the HttpContext.Connection.ClientCertificate
            });
        }

        // Apparantly we can use void when we dont want to do chaining (this parameter returned) instead of this type itself(IServiceCollection)
        public static void ConfigureLoggerService(this IServiceCollection services) => services.AddTransient<ILoggerManager, LoggerManager>();
        public static void ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            // services.AddDbContext<RepositoryContext>(options => options.UseSqlite(configuration.GetConnectionString(name: "sqlite")));

            // if we dont want to specify migration context and projects in commands we can use this code (the name of current Assembly should set here since by default it set the assembl containing the db context class)
            // we have to make this change since our context is not in our main project but in entity.csproj. Alternatively we can use the following command:
            // dotnet-ef migrations add migrationName -c contextname -p projectref
            // dotnet-ef migrations add migrationName --context contextname --project projectref
            services.AddDbContext<RepositoryContext>(options => options.UseSqlite(configuration.GetConnectionString(name: "sqlite"), b => b.MigrationsAssembly("WebAPI")));
        }
        public static void ConfigureRepositoryManager(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryManager, RepositoryManager>();
        }

        public static void PrintMediaFormattersDetail(this MvcOptions option)
        {
            Console.WriteLine("Default Output Formatters are:");
            foreach (var item in option.OutputFormatters)
            {
                var mediaFormatter = item as OutputFormatter;
                if (mediaFormatter == null)
                {
                    Console.WriteLine(item.ToString());
                }
                else
                {
                    Console.WriteLine($"{mediaFormatter.ToString()}:{String.Join(',', mediaFormatter.SupportedMediaTypes)}");
                }

            }
        }

        public static IMvcBuilder AddCustomCsvFormatter(this IMvcBuilder builder)
        {
            return builder.AddMvcOptions(options => options.OutputFormatters.Add(new CSVOutputFormatter()));
        }
    }
}