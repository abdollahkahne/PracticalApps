using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NorthwindIntl.ExceptionFilter;
using NorthwindIntl.RouteConstraint;
using NorthwindIntl.Transformer;
using NorthwindIntl.ValueProviders;
using Microsoft.Extensions.Caching.Redis;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NorthwindIntl.TagHelpers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Server.IISIntegration;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using NorthwindIntl.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Extensions.Primitives;

namespace NorthwindIntl
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
            // In App Development we should not depend on cache value and always assume that it may not be available!
            // Limite Cache Growth since it uses memory which is scarce (// 1-Dont create key from external input values 2- Set Size Limit on Cache totoal and every file 3-Use Expiration options)
            // we have two types of Memory Cache: Shared and Singleton. If you set size limited use the singleton since shared one used by all libraries
            // To Use In Memory/Distributed Cache We should register it
            // Apps running on a server farm (multiple servers) should ensure sessions are sticky when using the in-memory cache. 
            // Sticky sessions ensure that requests from a client all go to the same server.
            services.AddMemoryCache();
            // services.AddDistributedMemoryCache();
            // The distributed cache interface is limited to byte[] (And not string but string can be simply converted to Byte Array using Encoding.UTF8.GetBytes(str))
            // An app should manipulate cache values using an instance of IDistributedCache, not a SqlServerCache. So we should inject IDistribuedCache 
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = "localhost:6379";
                options.InstanceName = "Calculator";
            });

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                // Using windows Authentication
                services.AddAuthentication(IISDefaults.AuthenticationScheme);
            }
            else
            {
                // // Custom authentication using cookie
                // services
                // .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                // .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,options =>{
                //     options.LoginPath="/Account/Login";
                //     options.LogoutPath="/Account/Logout";
                //     options.AccessDeniedPath="/Account/Forbidden";
                //     options.ReturnUrlParameter="returnUrl";
                // });

                // // Using OpenIdConnect Providers like Azure
                // services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                // .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme,options => {
                //     options.Authority="";
                //     options.ClientId="";
                //     options.CallbackPath="";
                //     options.AccessDeniedPath="";
                // });

                // // Authentication using external providers
                // services.AddAuthentication(FacebookDefaults.AuthenticationScheme)
                // .AddFacebook(FacebookDefaults.AuthenticationScheme,options => {
                //     options.SignInScheme=CookieAuthenticationDefaults.AuthenticationScheme;
                //     // Other options are almost same to OAuth 
                // });

                // Authentication Using Identity
                services.AddDbContext<NorthwindIntlIdentityDbContext>(options =>
                    options.UseSqlite(Configuration.GetConnectionString("NorthwindIntlIdentityDbContextConnection"))
                ).AddDefaultIdentity<ApplicationUser>(option => { option.SignIn.RequireConfirmedAccount = false; })
                .AddEntityFrameworkStores<NorthwindIntlIdentityDbContext>();

                services.AddDatabaseDeveloperPageExceptionFilter();

                // // To do more configuration on default IdentityOptions, use Configure method directly
                // services.Configure<IdentityOptions>(options =>{
                //     // Password settings.
                //     options.Password.RequireDigit = true;
                //     options.Password.RequireLowercase = true;
                //     options.Password.RequireNonAlphanumeric = true;
                //     options.Password.RequireUppercase = true;
                //     options.Password.RequiredLength = 6;
                //     options.Password.RequiredUniqueChars = 1;

                //     // Lockout settings.
                //     options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                //     options.Lockout.MaxFailedAccessAttempts = 5;
                //     options.Lockout.AllowedForNewUsers = true;

                //     // User settings.
                //     options.User.AllowedUserNameCharacters =
                //     "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                //     options.User.RequireUniqueEmail = false;
                // });

                // // This is only need if we want to change the defaults
                // services.ConfigureApplicationCookie(options => {
                //     options.Cookie.HttpOnly=true;
                //     options.ExpireTimeSpan=TimeSpan.FromMinutes(30);
                //     options.SlidingExpiration=true;
                //     options.LoginPath="/Account/Login";
                //     options.LogoutPath="/Account/Logout";
                //     options.AccessDeniedPath="/Account/Forbidden";
                //     options.ReturnUrlParameter="returnUrl";
                // });

            }


            //Register TagHelperComponent
            services.AddTransient<ITagHelperComponent, HelloWorldTagHelperComponent>();

            // Register Filters that use DI using ServiceFilter attribute
            services.AddSingleton<LogFilter>();



            var supportedCultures = new List<CultureInfo> {
                new CultureInfo("en-US"),
                new CultureInfo("fa-IR"),
            };

            // config an option using Option Pattern from appsetting 
            services.Configure<PositionInfo>(Configuration.GetSection("Position"));

            // To use a filter in ServiceFilter we should register it
            services.AddScoped<FilterWithDI>();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.DefaultRequestCulture = new RequestCulture(supportedCultures.First().Name);
                options.RequestCultureProviders = new RequestCultureProvider[] {
                    new QueryStringRequestCultureProvider {QueryStringKey="culture"},
                    new AcceptLanguageHeaderRequestCultureProvider {Options=options,},
                };

            });
            // Add Localization
            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });

            // to Add session we should add it explicitly here and in configure method
            services.AddSession();

            services.AddSingleton<ITranslator, Translator>();
            services.AddSingleton<TranslateRouteValueTransformer>();

            //we should register the custome route constraints
            services.AddRouting(options =>
            {
                options.ConstraintMap.Add("evenint", typeof(EvenIntRouteConstraint));
            });

            //Resource Caching should be added to serivce collection (Client side response caching with two header Cache-Control and Pragma)
            // Add [ResponseCache] Attribute to action or controller 
            // Add UserResponseCaching Middleware too
            // Optionaly we can add Caching-Profile to MvcOptions if we use similar caching profile for some actions
            services.AddResponseCaching();

            // To decrease the size of response we can use response compression
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = false; // This is default because of security problems
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();

                // Apparantly the Append is used to Add items to IEnumerable which is OKay and work but it need that the source IEnumerable<T> be non empty. Here since options.MimeType is empty by default we could not use append method in this way (Correct)
                // options.MimeTypes.Append("img/svg+xml");// This is not true for IEnumerable since it may be otherthing than array or list (for example it can be output of a method with yield!)

                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "img/svg+xml" }); // Here we concat to Array and create new Array and Assign it to IEnumerable Option
                foreach (var item in options.MimeTypes)
                {
                    Console.WriteLine(item);
                }
            });

            // we can change compression level here. 3 options exist: fastest, optimal, no-compressed!
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = System.IO.Compression.CompressionLevel.Fastest;
            });
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = System.IO.Compression.CompressionLevel.Optimal;
            });

            // we should register IDeveloperPageExceptionFilter in case of Customizing that page
            // services.AddSingleton<IDeveloperPageExceptionFilter,CustomDeveloperPageExceptionFilter>();

            var mvc = services.AddControllersWithViews(option =>
            {

                var policy = new AuthorizationPolicyBuilder()
                .RequireAssertion(ctx => true)
                .Build();
                option.Filters.Add(new AuthorizeFilter(policy));

                // When passing an instance of a filter into Add, instead of its Type,
                // the filter is a singleton and is not thread-safe.
                option.Filters.Add<NotFoundAttribute>();

                option.ValueProviderFactories.Add(new CookieValueProviderFactory());
                // option.ModelBinderProviders.Insert(0,new HTMLEncodeModelBinderProvider());
                option.ModelValidatorProviders.Add(new MarriedPersonModelValidatorProvider());
                // option.AllowValidatingTopLevelNode=true; // This is by default true in 2.1 and later and removed from options apparantly
                // option.Filters.Add(new ValidateModelStateAttribute("/Home/error"));

                // Cache Profile is not necessary. We can set the caching setting directly too.
                option.CacheProfiles.Add("MyCacheProfile", new CacheProfile
                {
                    Duration = 5 * 60,
                    VaryByHeader = "Accept-Language",
                    Location = ResponseCacheLocation.Any,
                });

                var cacheProfiles = this.Configuration.GetSection("CacheProfiles").Get<Dictionary<string, CacheProfile>>();
                foreach (var item in cacheProfiles)
                {
                    option.CacheProfiles.Add(item.Key, item.Value);
                }

            })
            // .ConfigureApplicationPartManager(options =>{
            //     // This path should be .dll
            //     var path=Path.Combine(new string[] {"..","NorthwindEmployee","obj","Debug","net5.0","NorthwindEmployee.dll"});
            //     var asm=Assembly.LoadFrom(path);
            //     options.ApplicationParts.Add(new CompiledRazorAssemblyPart(asm));
            // })
            .AddRazorOptions(option =>
            {
                option.ViewLocationFormats.Add("/AdditionalViews/{1}/{0}.cshtml");
                option.ViewLocationExpanders.Add(new ThemeViewLocationExpander("Mastering"));
            }).AddViewOptions(option =>
            {
                option.HtmlHelperOptions.Html5DateRenderingMode = Html5DateRenderingMode.CurrentCulture;
            })
            .AddViewLocalization(format: Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix,
            setupAction: option => { option.ResourcesPath = "Resources"; });

#if DEBUG
            mvc.AddRazorRuntimeCompilation(); // This enable changing view in Runtime
#endif

            services.AddRazorPages(options =>
            {
                options.Conventions.AddPageRoute("/hellorazor", "Razor/{id}");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DiagnosticListener diagnostic)
        {
            // Altough We can Add LocalizationPipline Middleware Filter to Actions For View We should use this middleware here for now
            // If We want to apply the above configuration to Request Localization Option Here we should use it empty or directly config it here
            app.UseRequestLocalization();

            var listener = new MyDiagnosticListener();
            diagnostic.SubscribeWithAdapter(listener);


            app.UseResponseCompression();
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
            // app.UseExceptionHandler(option =>{
            //     option.Run(async ctx => {
            //         var errorFeature=ctx.Features.Get<IExceptionHandlerPathFeature>();
            //         var exception=errorFeature.Error;
            //         var path=errorFeature.Path;
            //         await ctx.Response.WriteAsync("Error: "+exception.Message+"\n"+"Path: "+path);
            //     });
            // });
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Work with windows Auth

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                app.Use((next) => ctx =>
                {

                    // windows Identity
                    var identity = WindowsIdentity.GetCurrent();
                    Console.WriteLine($"{identity.User.Value}");

                    var principal = new WindowsPrincipal(identity);
                    Console.WriteLine($"{principal.IsInRole(WindowsBuiltInRole.Administrator)}");
                    return next(ctx);
                });
            }
            else
            {
                app.UseAuthentication();
                app.Use(next => ctx =>
                {

                    var principal = ctx.User;
                    if (principal != null) { return next(ctx); }
                    Console.WriteLine($"Is Admin:{principal.IsInRole("admin")}");
                    var identity = principal.Identity;
                    Console.WriteLine($"{identity.Name} is {principal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role).Value}");
                    return next(ctx);
                });
            }



            app.UseAuthorization();
            // To handle VaryByQueryKey using the ResponseCachingFeature directly from the HttpContext.Features:
            app.Use(async (context, next) =>
            {
                //get feature
                var responseCachingFeature = context.Features.Get<IResponseCachingFeature>();
                if (responseCachingFeature != null)
                {
                    // If you want to do the caching vary by all query string keys use *
                    responseCachingFeature.VaryByQueryKeys = new String[] { "productName" };// new StringValues("productName");
                }
            });
            // As Microsoft this do not save/store response in server itself but I think it use memory to save and its default size is 100MB which full with two picturs at most!
            // The only header that explicitly handle by this is VaryByQueryKey which does not have a exist in header (Cookie exist in response headers in set-cookie)
            app.UseResponseCaching();// This should call after UseCors and I think after authorization too but better approach is to do not use CacheResponse attribute with authorized contents. Also
            // if Authorization header exists: The response isn't cached if the header exists.
            // Set-Cookie: The response isn't cached if the header exists. for session and authentication or for example for flushing and temp data providing
            // Vary: A response with a header value of * is never stored if vary existed in the request header
            // Date: if not presented in original headers of respponse, when response is sent from cache a Date header is added
            // A browser may set the Cache-Control header to no-cache or max-age=0 when refreshing a page. So we have respect the clients cache request. 
            // To see that caching also done in server you can use tools like postman or fiddler.

            // This midlleware defines Header value regarding to cache-control in client side which include typed headers like cache-control itself and array like or string type simple headers
            //Here we can add Headers for response related to caching but this header overwriten by related filter if we using CacheResponse Attribute in action/page or controller level
            // Why we added this? I think only to show that header are ovrwriten and should remove in real codes
            app.Use(next => async context =>
             {

                 context.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
                 {
                     // The existed properties here: some of the apply to request too.
                     // max-age
                     // max-stale: request by client and also exist in response to show the freshness of response cached in second
                     // min-fresh
                     // must-revalidate
                     // no-cache
                     // no-store
                     // only-if-cached
                     // private
                     // public
                     // s-maxage
                     // proxy-revalidate‡
                     Public = true,
                     MaxAge = TimeSpan.FromSeconds(10),
                 }; // with this method we can get headers which is not in Header and have typing for example cach:public;max-age=10
                    // Other way is directly from Headers itself
                    // All Header types defined in HeadersName class
                 context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] = new Microsoft.Extensions.Primitives.StringValues("Accept-Encoding"); // simply we can set new string[] {"Accept-Encoding"};
                 await next(context);
             });

            // app.Use(next =>async ctx =>{
            //     foreach (var item in ctx.Features)
            //     {
            //         Console.WriteLine(item);
            //     }
            //     await next(ctx);
            // });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "calculator",
                pattern: "_api/{controller}/{action}",
                defaults: new { controller = "Calculator", action = "Calculate" },
                constraints: new { controller = new StringRouteConstraint("Calculator") },
                dataTokens: new { Foo = "Bar" }
                );

                // endpoints.MapDynamicControllerRoute<TranslateRouteValueTransformer>(
                //     pattern: "{language=en}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapDefaultControllerRoute();

                // We can write Request delegate directly in MapGet Or Use middleware chain in 
                // a new instance of App Builder and then build it to generate a request delegate
                var builder = endpoints.CreateApplicationBuilder();
                builder.Use(next => async ctx => { ctx.Response.StatusCode = 201; await next(ctx); });
                builder.Use(next => async ctx => await ctx.Response.WriteAsync("Hello From Middleware!"));
                endpoints.MapGet("middleware", builder.Build());
                endpoints.MapRazorPages();
            });
        }
    }
}

// Conditions for caching
//     The request must result in a server response with a 200 (OK) status code. Other status codes does not cache
//     The request method must be GET or HEAD. So Posting form arent cache
//     In Startup.Configure, Response Caching Middleware must be placed before middleware that require caching. so static file does not cache by server in our example which is good since they already are static and easy to send
//     The Authorization header must not be present. So we have not have Vary by Authorization!
//     Cache-Control header parameters must be valid, and the response must be marked public and not marked private.
//     The Pragma: no-cache header must not be present if the Cache-Control header isn't present, as the Cache-Control header overrides the Pragma header when present.
//     The Set-Cookie header must not be present.
//     Vary header parameters must be valid and not equal to *.
//     The Content-Length header value (if set) must match the size of the response body.
//     The IHttpSendFileFeature isn't used. for static files this is usefull if they send with send file
//     The response must not be stale as specified by the Expires header and the max-age and s-maxage cache directives.
//     Response buffering must be successful. The size of the response must be smaller than the configured or default SizeLimit. The body size of the response must be smaller than the configured or default MaximumBodySize.
//     The response must be cacheable according to the RFC 7234 specifications. For example, the no-store directive must not exist in request or response header fields. See Section 3: Storing Responses in Caches of RFC 7234 for details.
