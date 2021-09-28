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

            

            var supportedCultures=new List<CultureInfo> {                
                new CultureInfo("en-US"),
                new CultureInfo("fa-IR"),
            };

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

                
            }).AddRazorOptions(option =>{
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
