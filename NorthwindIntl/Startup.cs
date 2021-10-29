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

            // To Use In Memory/Distributed Cache We should register it
            services.AddMemoryCache();
            // services.AddDistributedMemoryCache();
            services.AddDistributedRedisCache(options =>{
                options.Configuration="localhost:6379";
                options.InstanceName="Calculator";
            });

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)) {
                // Using windows Authentication
                services.AddAuthentication(IISDefaults.AuthenticationScheme);
            } else {
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
                ).AddDefaultIdentity<ApplicationUser>(option => {option.SignIn.RequireConfirmedAccount=false;})
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
            services.AddTransient<ITagHelperComponent,HelloWorldTagHelperComponent>();

            // Register Filters that use DI using ServiceFilter attribute
            services.AddSingleton<LogFilter>();

            

            var supportedCultures=new List<CultureInfo> {                
                new CultureInfo("en-US"),
                new CultureInfo("fa-IR"),
            };

            // config an option using Option Pattern from appsetting 
            services.Configure<PositionInfo>(Configuration.GetSection("Position"));

            // To use a filter in ServiceFilter we should register it
            services.AddScoped<FilterWithDI>();

            services.Configure<RequestLocalizationOptions>(options =>{
                options.SupportedCultures=supportedCultures;
                options.SupportedUICultures=supportedCultures;
                options.DefaultRequestCulture=new RequestCulture(supportedCultures.First().Name);
                options.RequestCultureProviders=new RequestCultureProvider[] {
                    new QueryStringRequestCultureProvider {QueryStringKey="culture"},
                    new AcceptLanguageHeaderRequestCultureProvider {Options=options,},
                };

            });
            // Add Localization
            services.AddLocalization(options =>{
                options.ResourcesPath="Resources";
            });

            // to Add session we should add it explicitly here and in configure method
            services.AddSession();

            services.AddSingleton<ITranslator,Translator>();
            services.AddSingleton<TranslateRouteValueTransformer>();

            //we should register the custome route constraints
            services.AddRouting(options =>{
                options.ConstraintMap.Add("evenint",typeof(EvenIntRouteConstraint));
            });

            //Resource Caching should be added to serivce collection
            services.AddResponseCaching();

            // we should register IDeveloperPageExceptionFilter in case of Customizing that page
            // services.AddSingleton<IDeveloperPageExceptionFilter,CustomDeveloperPageExceptionFilter>();


            var mvc=services.AddControllersWithViews(option =>{

                var policy=new AuthorizationPolicyBuilder()
                .RequireAssertion(ctx =>true)
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
                option.CacheProfiles.Add("MyCacheProfile",new CacheProfile {
                    Duration=5*60,
                    VaryByHeader="Accept-Language",
                    Location=ResponseCacheLocation.Any,
                });

                
            })
            // .ConfigureApplicationPartManager(options =>{
            //     // This path should be .dll
            //     var path=Path.Combine(new string[] {"..","NorthwindEmployee","obj","Debug","net5.0","NorthwindEmployee.dll"});
            //     var asm=Assembly.LoadFrom(path);
            //     options.ApplicationParts.Add(new CompiledRazorAssemblyPart(asm));
            // })
            .AddRazorOptions(option =>{
                option.ViewLocationFormats.Add("/AdditionalViews/{1}/{0}.cshtml");
                option.ViewLocationExpanders.Add(new ThemeViewLocationExpander("Mastering"));
            }).AddViewOptions(option =>{
                option.HtmlHelperOptions.Html5DateRenderingMode=Html5DateRenderingMode.CurrentCulture;
            })
            .AddViewLocalization(format:Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix,
            setupAction:option =>{option.ResourcesPath="Resources";});

            # if DEBUG
            mvc.AddRazorRuntimeCompilation(); // This enable changing view in Runtime
            # endif

            services.AddRazorPages(options =>{
                options.Conventions.AddPageRoute("/hellorazor","Razor/{id}");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DiagnosticListener diagnostic)
        {
            // Altough We can Add LocalizationPipline Middleware Filter to Actions For View We should use this middleware here for now
            // If We want to apply the above configuration to Request Localization Option Here we should use it empty or directly config it here
            app.UseRequestLocalization();

            var listener=new MyDiagnosticListener();
            diagnostic.SubscribeWithAdapter(listener);

            app.UseResponseCaching();
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

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)) {
                app.Use((next)=>ctx =>{

                // windows Identity
                var identity=WindowsIdentity.GetCurrent();
                Console.WriteLine($"{identity.User.Value}");

                var principal=new WindowsPrincipal(identity);
                Console.WriteLine($"{principal.IsInRole(WindowsBuiltInRole.Administrator)}");
                return next(ctx);
                });
            } else {
                app.UseAuthentication();
                app.Use(next =>ctx =>{
                    
                    var principal=ctx.User;
                    if (principal!=null) { return next(ctx);}
                    Console.WriteLine($"Is Admin:{principal.IsInRole("admin")}");
                    var identity=principal.Identity;
                    Console.WriteLine($"{identity.Name} is {principal.Claims.SingleOrDefault(c=>c.Type==ClaimTypes.Role).Value}");
                    return next(ctx);
                });
            }

            

            app.UseAuthorization();

            // app.Use(next =>async ctx =>{
            //     foreach (var item in ctx.Features)
            //     {
            //         Console.WriteLine(item);
            //     }
            //     await next(ctx);
            // });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name:"calculator", 
                pattern:"_api/{controller}/{action}",
                defaults:new {controller="Calculator",action = "Calculate"},
                constraints:new {controller= new StringRouteConstraint("Calculator")},
                dataTokens:new {Foo ="Bar"}
                );

                // endpoints.MapDynamicControllerRoute<TranslateRouteValueTransformer>(
                //     pattern: "{language=en}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapDefaultControllerRoute();

                // We can write Request delegate directly in MapGet Or Use middleware chain in 
                // a new instance of App Builder and then build it to generate a request delegate
                var builder=endpoints.CreateApplicationBuilder();
                builder.Use(next=>async ctx=> {ctx.Response.StatusCode=201;await next(ctx);});
                builder.Use(next=>async ctx=> await ctx.Response.WriteAsync("Hello From Middleware!"));
                endpoints.MapGet("middleware",builder.Build());
                endpoints.MapRazorPages();
            });
        }
    }
}
