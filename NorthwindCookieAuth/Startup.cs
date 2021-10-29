using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NorthwindCookieAuth.Authentication;
using NorthwindCookieAuth.Authorization;
using NorthwindCookieAuth.Diagnostics;
using NorthwindCookieAuth.EventSources;
using NorthwindCookieAuth.Extensions;
using NorthwindCookieAuth.Middlewares;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using PracticalApp.NorthwindContextLib;
using Microsoft.EntityFrameworkCore;
using NorthwindCookieAuth.HostedServices;

namespace NorthwindCookieAuth
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
            // This health check is set outside itself in a hosted service
            services.AddSingleton<StartupHostedServiceHealthCheck>(); // This should be singleton since it use in health check too. 

            services.AddHostedService<StartupHostedService>();

            services.AddDbContext<Northwind>(options => { options.UseSqlite(Configuration.GetConnectionString("ApplicationConnection")); });
            // The app is considered healthy if it's capable of responding at the health endpoint URL
            services.AddHealthChecks().AddMemoryHealthCheck("memory_health_check")// we defined AddMemoryHealthCheck As Extension here
            // .AddCheck<ExampleHealthCheck>("Example_Health_Check")
            .AddCheck<ExampleHealthCheck>("Example_Health_Check", HealthStatus.Degraded, new[] { "Example", "Failure Status" })
            .AddTypeActivatedCheck<ExampleHealthCheckWithArgs>(
                name: "Example_health_Check_with_Args",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "Example", "Arguments" },
                args: new[] { "https://bing.com" }
            )
            .AddCheck("Sample Lambda", () => HealthCheckResult.Healthy("All is ok!")) // This check Healths which writen here
            .AddSqlite(Configuration["ConnectionStrings:DefaultConnection"]) // This check Database Connection Health
            .AddDbContextCheck<Northwind>() // Check db connection using EF Core DbContext
            .AddCheck<StartupHostedServiceHealthCheck>("hosted_service_startup", failureStatus: HealthStatus.Degraded, tags: new[] { "HOSTED", "Example", "Ready", "Readiness" });

            services.AddSingleton<IHealthCheckPublisher, ReadinessPublisher>();
            services.Configure<HealthCheckPublisherOptions>(option =>
            {
                option.Delay = TimeSpan.FromSeconds(20);
                option.Predicate = (check) => check.Tags.Contains("Ready");
            });



            services.AddElm();
            services.AddMiddlewareAnalysis();
            services.AddLogging(options =>
            {
                options.AddConsole().AddDebug();
                options.AddFileLogger(LogLevel.Information);
            });
            services.AddSession(options =>
            {
                // Session state cookies are not essential. Session state isn't functional when tracking is disabled.
                // The following code makes session cookies essential:
                options.Cookie.IsEssential = true;
            });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                // Here we can set Authentication Options for Cookie like login,logout and accessdenied paths
                // The cookie-sending behavior defined in options.Cookie.SameSite which can be lax which allow sending to different 
                // If Option.Session is not specified the user information is serialized/encrypted and stored in the cookie.
                // SignInAsync extension method for HttpContext creates an encrypted cookie and adds it to the current response
                // ASP.NET Core's Data Protection system is used for encryption.
                // For an app hosted on multiple machines, load balancing across apps, or using a web farm,
                // configure data protection to use the same key ring and app identifier.
                // Once a cookie is created, the cookie is the single source of identity.
                // If a user account is disabled in back-end systems:
                // The app's cookie authentication system continues to process requests based on the authentication cookie.
                // The user remains signed into the app as long as the authentication cookie is valid.
                // -- If we want to overcome this we can use  ValidatePrincipal event  to validate on every request
                // An optimized version here is to have a claim equal to Modified Time and use that in the ValidatePrincipal Event
                // To define this service events we should define its event type in a custom class or an istance of it to events
                options.EventsType = typeof(CustomAuthenticationEvent);
            });

            services.Configure<CookiePolicyOptions>(option =>
            {
                option.CheckConsentNeeded = (ctx) => true;
            });

            // The TempData provider cookie is not essential. Make it essential
            // so TempData is functional when tracking is disabled.
            // This is approach used by TempData Dictionary for persisting data between request when session is not available
            services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.IsEssential = true;
            });

            services.AddScoped<CustomAuthenticationEvent>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AuthenticatedUser", builder => { builder.RequireAuthenticatedUser(); });
                options.AddPolicy("Claim-Based", builder => builder.RequireClaim("Age", new[] { "20", "21", "22" }));
                options.AddPolicy("Adults", builder =>
                {
                    builder.RequireAssertion(ctx =>
                    {
                        var age = int.Parse(ctx.User.Claims.SingleOrDefault(c => c.Type == "Age")?.Value);
                        return age >= 18;
                    });
                });
                // If a handler calls context.Succeed or context.Fail, all other handlers are
                // still called. This allows requirements to produce side effects,
                // such as logging, which takes place even if another handler has
                // successfully validated or failed a requirement. When set to false,
                // the InvokeHandlersAfterFailure property short-circuits the execution of handlers when context.Fail is called. 
                options.InvokeHandlersAfterFailure = false;
            });

            services.AddSingleton<IAuthorizationHandler, MultiRequirementAuthorizationhandler>();

            services.AddSingleton<IAuthorizationPolicyProvider, MinimumAgePolicyProvider>();
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

            services.AddRazorPages(options =>
            {
                options.Conventions.AllowAnonymousToPage("/Account/Login");
                options.Conventions.AuthorizePage("/privacy", "AuthenticatedUser");
                // Alternatively, An AuthorizeFilter can be applied to a page model class with the [Authorize] filter attribute.

                // options.Conventions.AuthorizeFolder("/Northwind").AllowAnonymousToPage("/Northwind/Index");
                // It's valid to specify that a folder of pages requires authorization and
                // then specify that a page within that folder allows anonymous access.
                // The reverse, however, isn't valid. You can not authorize page after allow anynomous a folder
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DiagnosticListener diagnostic)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseElmPage();
                app.UseElmCapture();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // app.UseLoggingMiddleware();

            // we should hook up our listener and middleware pipeline in our Startup.Configure method:
            var myListener = new DemoDiagnosticListener();
            diagnostic.SubscribeWithAdapter(myListener);
            // A DiagnosticListener is injected into the Configure method from the DI container. This is the actual class that is used to subscribe to diagnostic events.
            // We use the SubscribeWithAdapter extension metho to register our DemoDiagnosticListener.
            // This hooks into the [DiagnosticName] attribute to register our events (Diagnostic Listeners), so that the listener is invoked when the event is fired afterward.

            app.UseMiddleware<DiagnosticMiddleware>();

            // var middlewareListener=new TraceDiagnosticListener();
            // diagnostic.SubscribeWithAdapter(middlewareListener);

            // app.UseStaticFiles();

            app.UseRouting();

            app.UseCookiePolicy();

            app.UseAuthentication(); // This set HttpContext.User

            app.UseAuthorization(); // This check user has authorization or not?

            app.UseProtectStaticPath(new ProtectStaticPathOptions
            {
                Path = "/static/Group1",
                PolicyName = "AuthenticatedUser",
            }, new ProtectStaticPathOptions
            {
                Path = "/static/Group2",
                PolicyName = "Claim-Based"
            });
            app.UseStaticFiles();

            // Properties is a key/value collection that can be used to share data between middleware.
            // [analysis.NextMiddlewareName] is used in Diagnostic Logging as name parameter
            app.Properties["analysis.NextMiddlewareName"] = "Anynomous Middleware";
            app.Use(next => async ctx => { await next(ctx); });

            app.MapWhen(
                ctx => ctx.Request.Method == HttpMethods.Get && ctx.Request.Path.StartsWithSegments("/_api/health"),
                builder => builder.UseHealthChecks(
                    null,
                    new HealthCheckOptions { ResponseWriter = WriteHealtchCheckResponse }
                )
            );

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    // Predicate = registeration => registeration.Tags.Contains("Example"),
                    ResultStatusCodes ={
                        [HealthStatus.Healthy]=StatusCodes.Status200OK,
                        [HealthStatus.Degraded]=StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy]=StatusCodes.Status503ServiceUnavailable,
                    },
                    // ResponseWriter = WriteHealtchCheckResponse,
                    ResponseWriter = WriteResponseUsingNewton,
                    AllowCachingResponses = false, // f the value is false (default), the middleware sets or overrides the Cache-Control, Expires, and Pragma headers to prevent response caching. 
                })
                .RequireAuthorization(policyNames: new string[] { "AuthenticatedUser" })
                .RequireHost($"*:{Configuration["ManagementPort"]}");

                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions()
                {
                    Predicate = (check) => check.Tags.Contains("ready"),
                });

                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions()
                {
                    Predicate = (_) => false
                });

                // Create and delete Database from Db Context Programmaticly
                endpoints.MapGet("/createdatabase", async context =>
                {
                    await context.Response.WriteAsync("Creating the database...");
                    await context.Response.WriteAsync(Environment.NewLine);
                    await context.Response.Body.FlushAsync();

                    var appDbContext =
                        context.RequestServices.GetRequiredService<Northwind>();
                    await appDbContext.Database.EnsureCreatedAsync();

                    await context.Response.WriteAsync("Done!");
                    await context.Response.WriteAsync(Environment.NewLine);
                    await context.Response.WriteAsync(
                        "Navigate to /health to see the health status.");
                    await context.Response.WriteAsync(Environment.NewLine);
                });

                endpoints.MapGet("deletedatabase", async context =>
                {
                    await context.Response.WriteAsync("Deleting the database...");
                    await context.Response.WriteAsync(Environment.NewLine);
                    await context.Response.Body.FlushAsync();

                    var appDbContext =
                        context.RequestServices.GetRequiredService<Northwind>();
                    await appDbContext.Database.EnsureDeletedAsync();

                    await context.Response.WriteAsync("Done!");
                    await context.Response.WriteAsync(Environment.NewLine);
                    await context.Response.WriteAsync("Navigate to /health to see the health status.");
                    await context.Response.WriteAsync(Environment.NewLine);
                });
            });
        }
        private static Task WriteHealtchCheckResponse(HttpContext context, HealthReport report)
        {
            // report is an Object of type HealthReport. It has an status property and A dictionary field which has an entry for each Healtch Check
            // report.Entries is the dictionary corrosponding to each health check which has key equal to check name and a HealthReportEntry type value
            // Each value has a status property related to the health check and a Value field which contain data related to the check
            // This value has a Data field is an Object containing 
            // a 
            // 
            context.Response.ContentType = "application/json;charset=utf-8";
            var options = new JsonWriterOptions
            {
                Indented = true,
            };

            using (var ms = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(ms, options))
                {
                    writer.WriteStartObject();
                    writer.WriteString("status", report.Status.ToString());
                    writer.WriteStartObject("report");
                    foreach (var item in report.Entries)
                    {
                        writer.WriteStartObject(item.Key);
                        writer.WriteString("status", item.Value.Status.ToString());
                        writer.WriteString("description", item.Value.Description);
                        writer.WriteStartObject("data");
                        foreach (var d in item.Value.Data)
                        {
                            writer.WritePropertyName(d.Key);
                            JsonSerializer.Serialize(writer, d.Value, d.Value.GetType() ?? typeof(Object));
                        }
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                    }
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                    // JsonSerializer.Serialize(writer, report, typeof(Object));
                }
                var json = Encoding.UTF8.GetString(ms.ToArray());
                return context.Response.WriteAsync(json);
            }
        }

        private static Task WriteResponseUsingNewton(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json;charset=utf-8";
            // Arrays are JObject and Key-Values are JProperty. 
            var json = new JObject(
                new JProperty("status", report.Status.ToString()),
                new JProperty("results", new JObject(report.Entries.Select(health =>
                     new JProperty(health.Key, new JObject(
                         new JProperty("status", health.Value.Status),
                         new JProperty("description", health.Value.Description),
                         new JProperty("data", new JObject(health.Value.Data.Select(d => new JProperty(d.Key, d.Value)))
                     ))
                 )))
                )
            );
            return context.Response.WriteAsync(json.ToString(Newtonsoft.Json.Formatting.Indented));
        }
    }
}
