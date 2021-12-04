using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Src.Data;
using Src.Data.IdentityModels;
using Src.Data.TagHelperModel;
using Src.IdentityPolicies;

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

            services.AddAuthorization(options =>
            {
                options.AddPolicy("SkilledManager", policyBuilder =>
                {
                    policyBuilder.RequireRole("Admin");
                    policyBuilder.RequireClaim("Skill");
                });
            });

            services.AddSingleton<IProductRepository, ProductRepository>();
            // services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("InMemory"));
            services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=App1.db"));


            // Here AddDefaultIdentity include multiple services include Identity Token Provider which
            // used to generate tokens for reset passwords, change email and change telephone number operations, and for two factor authentication token generation.
            services.AddDefaultIdentity<AppUser>(options => { options.Password.RequireUppercase = true; })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddPasswordValidator<CustomPasswordPolicy>()
            .AddUserValidator<CustomUsernameEmailPolicy>();
            // You can Always ad more configuration using service.Configure 
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
            });

            // Register Identity Password Policies Here:
            // services.AddScoped<IPasswordValidator<AppUser>, CustomPasswordPolicy>();
            // services.AddScoped<IUserValidator<AppUser>, CustomUsernameEmailPolicy>();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Authenticate/Login";
                options.AccessDeniedPath = "/Authenticate/AccessDenied";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(2);
            });

            services.AddAuthentication(
            //     options =>
            // {
            //     options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            // options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //     options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //     options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            // }
            )
            .AddGoogle(options =>
            {
                // In OAuth authentication there is following step usually
                // 1- In Browser user redirect to The Authorization EndPoint (Client ID used here) of provider and login
                // 2-Provider redirect to the CallBackUrl with a temporary code 
                // 3- Server process the request and send a request to The Token EndPoint (Client Secret and temporary Code parameter used here) and return a Token and Some basic user info here!
                // 4- (Only in Oauth and not in OpenIDConnect) server use the token to request further user information and make further request to the provider resources
                // All of the setting can be set here
                // options.AuthorizationEndpoint="https://github.com/login/oauth/authorize";// This can be set in the hyperlink manually to but we can use Challange to go to this link
                // options.TokenEndpoint="https://github.com/login/oauth/access_token"; This used by Authentication Middleware in step 3
                // options.UserInformationEndpoint="https://api.github.com/user";// not called by the OAuth authentication handler itself, but instead is something we need to call to obtain more information about the user

                // For doing Authentication we need to create a claim principal with at least two claim type:Name,NameIdentifier. The response which we receive in step 3 or 4 has this claim but it needs to map them
                // options.ClaimActions.MapJsonKey(ClaimTypes.Name, "related json key from response");
                // options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                // options.ClaimActions.MapJsonKey("urn:github:login", "login");
                // options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
                // options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");

                // // Now we should feed the above claim actions with a json. This json returned as a result to step 4 (Or may be 3). So we should hook to them
                // options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents()
                // {
                //     // This is the step 4 and not done automataicly
                //     OnCreatingTicket = async (ctx) =>
                //     {
                //         // make the request 
                //         var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
                //         request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                //         request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ctx.AccessToken);
                //         // send it and get the response
                //         var response = await ctx.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ctx.HttpContext.RequestAborted);
                //         response.EnsureSuccessStatusCode();
                //         // feed the response to claim actions
                //         var user = JObject.Parse(await response.Content.ReadAsStringAsync()); // JObject is part of Newton Json
                //         ctx.RunClaimActions(user); // the user used in claim action defined above
                //     },
                // };



                var googleConfig = Configuration.GetSection("Google");
                options.ClientId = googleConfig["ClientID"];
                options.ClientSecret = googleConfig["ClientSecret"];
                options.SignInScheme = IdentityConstants.ExternalScheme; // This is the schema that used for persisting user info for this case in cookie. 
                // options.CallbackPath = "/Account/GoogleResponse";
                // options.CorrelationCookie = new Microsoft.AspNetCore.Http.CookieBuilder { SecurePolicy = CookieSecurePolicy.None };

                // Here we can create a mapping between Claims Provided by Google and our defined claim
                // We should save them if we want to make the persist over multiple request
                options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
                options.ClaimActions.MapJsonKey("urn:google:locale", "locale", "string");
                options.ClaimActions.MapJsonKey("urn:google:user:birthday", "birthday", "string");

                // SaveTokens is set to false by default to reduce the size of the final authentication cookie (Important to keep cookie size small. Claims are also save in cookie, so we should have as small as needed)
                options.SaveTokens = true; // this save the token in cookie.  we can retrieve the access token by calling the GetTokenAsync() method
                // The Events include multiple events: Here we used the OnCreatingTicket which Invoked after the provider successfully authenticates a user
                options.Events.OnCreatingTicket = ctx =>
                {
                    List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList();

                    tokens.Add(new AuthenticationToken()
                    {
                        Name = "TicketCreated",
                        Value = DateTime.UtcNow.ToString()
                    });

                    ctx.Properties.StoreTokens(tokens);

                    return Task.CompletedTask;
                };
                // Add more scope
                options.Scope.Add("https://www.googleapis.com/auth/user.birthday.read");
            });

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseDeveloperExceptionPage();

            // app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
        }
    }
}
