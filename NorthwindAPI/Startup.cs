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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PracticalApp.NorthwindAPI.Utilities;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using PracticalApp.NorthwindEntitiesLib;

namespace PracticalApp.NorthwindAPI
{
    public class Startup
    {
        private static IEdmModel GetEdmModel() {
            var builder=new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("CustomersOdata");
            builder.EntitySet<Supplier>("Suppliers");
            return builder.GetEdmModel();
        }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            

            services.AddCors();

            services.AddApiVersioning(options => {
                options.ReportApiVersions=true;
                // options.RouteConstraintName="apiversion";
                // options.DefaultApiVersion=ApiVersion.Parse("2.0");
                options.DefaultApiVersion=new ApiVersion(2,0);
                options.AssumeDefaultVersionWhenUnspecified=true;
                options.ApiVersionReader=new HeaderApiVersionReader("api-version");
                // options.ApiVersionReader=new QueryStringApiVersionReader("version");
                // options.ApiVersionReader=new UrlSegmentApiVersionReader(); // the segment should be constraint using apiversion

            });


            services.AddAuthentication(options =>{
                options.DefaultAuthenticateScheme= JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme=JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme=JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,options =>{
                options.TokenValidationParameters=new TokenValidationParameters {
                    ValidateAudience=false,
                    ValidateIssuer=false,
                    ValidateIssuerSigningKey=true,
                    IssuerSigningKey=AuthenticationExtensions.Key,
                    ValidateLifetime=true,
                    ClockSkew=TimeSpan.FromMinutes(5),

                };
            });

            // we can configure ApiBehaviorOptions to change default behaviour enforced by [ApiController] attribute
            // services.Configure<ApiBehaviorOptions>(option =>{
            //     option.SuppressModelStateInvalidFilter=true;
            // });


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
            .AddOData(options =>{
                options.AddRouteComponents("odata",GetEdmModel());
                options.Select().Expand().Filter().OrderBy().Count().SetMaxTop(5);
            })
            .AddXmlDataContractSerializerFormatters()
            .AddXmlSerializerFormatters()
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "NorthwindAPI", Version = "2.0" });
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NorthwindAPI", Version = "1.0" });
                c.ResolveConflictingActions(apiDesc =>apiDesc.First());
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
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NorthwindAPI v1");
                    c.SwaggerEndpoint("/swagger/v2/swagger.json", "NorthwindAPI v2");
                });
                app.UseHealthChecks(path: "/health-check");
            }

            app.UseExceptionHandler(builder =>{
                builder.Run(async context=>{
                var exceptionHandlerPathFeature =context.Features.Get<IExceptionHandlerPathFeature>();
                var exception =exceptionHandlerPathFeature.Error;
                var path=exceptionHandlerPathFeature.Path;
                var problemDetails=new ProblemDetails {
                    Instance=$"urn:my:error:{Guid.NewGuid()}",
                    Detail=exception.Message,
                };
                if (exception is BadHttpRequestException bad) {
                    problemDetails.Title="Invalid Request";
                    problemDetails.Status=StatusCodes.Status400BadRequest;
                } else {
                    problemDetails.Title="An uexpected error happened";
                    problemDetails.Status=StatusCodes.Status500InternalServerError;
                }
                context.Response.StatusCode=problemDetails.Status.Value;
                context.Response.ContentType="application/problem+json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
                });
                
            });

            app.UseHttpsRedirection();           

            app.UseRouting();
            app.UseCors(options =>
            {
                options.WithMethods("GET", "POST", "DELETE", "PUT");
                options.WithOrigins("https://localhost:5002");
            });
            app.UseAuthentication();
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
                
                // If we have not async-await or next to return a Task we can use the following:
                // return Task.CompletedTask;
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
