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
using SignalRSample.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalRSample.SignalR;
using Microsoft.AspNetCore.SignalR;
using SignalRSample.SignalR.Filters;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace SignalRSample
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

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddSignalR(options =>
            {
                options.AddFilter<LanguageHubFilter>();// Add Global Filter Here

            })
            .AddHubOptions<ChatHub>(options =>
            {
                // The Hub class should be added as generic here.
                options.AddFilter<MyHubFilter>();// Add Local Filters Here

                //** Options Related to Hub/Connection Management in Server/Hub
                // Time outs
                options.KeepAliveInterval = TimeSpan.FromSeconds(15); // Default is 15s. Every 15 second server send a ping to client to keep connection alive (this only sends in case of no custome event ping in every 15s)
                options.HandshakeTimeout = TimeSpan.FromSeconds(15); // If the client does not send handshake after 15 second from openning the connection from client, (Try not change this it is advanced!)
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30); // If server does not receive ping/message every 30s from client, consider it disconnected and close the connection (usually double of keep alive timeout)

                // Message Protocol
                foreach (var item in options.SupportedProtocols) // This means message protocols like json,msgpack and by default include all oth them
                {
                    Console.WriteLine(item);
                }

                // limits
                options.MaximumReceiveMessageSize = 32 * 8 * 1024; // Maximum size of every incomming message. The default is 32KB
                options.MaximumParallelInvocationsPerClient = 1; // this limit the number of concurrent events triggered by clients (methods invoked by clients) before Server queue them and do them sequentially

                // Stream
                options.StreamBufferCapacity = 10; //?

                //Exceptions:
                // Exception messages are generally considered sensitive data that shouldn't be revealed to a client.
                // By default, SignalR doesn't send the details of an exception thrown by a hub method to the client.
                // Instead, the client receives a generic message indicating an error occurred. 
                options.EnableDetailedErrors = true; //  Exception messages should not be exposed to the client in production apps.

            }).AddHubOptions<StreamingHub>(options =>
            {
                options.EnableDetailedErrors = true;
            }).AddJsonProtocol(options =>
            {
                // Serialization options which are similar to other components
                options.PayloadSerializerOptions.AllowTrailingCommas = true;
                options.PayloadSerializerOptions.PropertyNameCaseInsensitive = false;
                options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            }).AddStackExchangeRedis(redisConnectionString: "localhost:6379"); // we only can have one signalR service in redis so it set its name. for case of using same using multiple app set ChannelPrefix in configuration to name of app
            services.AddSingleton<IHubFilter, MyHubFilter>();
            services.AddSingleton<IHubFilter, LanguageHubFilter>();
            services.AddSingleton<IUserIdProvider, NameUserIdProvider>(); // This is used to define the unique identifier of users which can be an id, an email or username

            // To add authentication from other places than Authorization Header, We should hook to an event name OnMessageReceived (specially for WebSocket and Sever Sent Events which header does not allow)
            services.AddAuthentication().AddJwtBearer(options =>
            {
                // We should have an authority to check the token in exchange of user data (It can be a url or ourself do singing and reading here manually)
                options.Authority = "Ourself";
                options.ClaimsIssuer = "SignalR App";
                // We have to hook the OnMessageReceived event in order to
                // allow the JWT authentication handler to read the access
                // token from the query string when a WebSocket or 
                // Server-Sent Events request comes in.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = (ctx) =>
                    {
                        // Altough we have to use the query string for authentication but it has some security consideration (for example in http it is not secured)
                        // When using HTTPS, query string values are secured by the TLS connection. However, many servers log query string values.
                        // As an alternative we can use single use of it which generated from cookie-based authentication which used on other component of our App
                        var accessToken = ctx.HttpContext.Request.Query["access_token"];
                        var path = ctx.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                        {
                            ctx.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("DevelopersAdmin", builder => builder.AddRequirements(new RestrictedRequirement()));
            });
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
            // app.UseWebSockets(); // An alternative to SignalR
            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            // When getting the page that contain Hub Logic two url called at least
            // The first is the http page which is static here /chat
            // second is the hub request when building Hub Connection
            // The following Middleware only run for second request
            app.Use(next => async (ctx) =>
              {
                  Console.WriteLine(ctx.Request.Method.ToString());
                  Console.WriteLine(ctx.Request.Path.Value);
                  Console.WriteLine(ctx.Request.QueryString.ToString());
                  //   var endpoint = ctx.GetEndpoint();
                  //   if (endpoint != null)
                  //   {
                  //       // This runs twice (not Three times!)
                  //       // 1- negotiation (this is named some times websocket handshaking too)=> it is http and if successful as a result connection id and connection token is returned. Also its available Transports and transfer format for each of them is specified:some times this step ignore using skipNegotiation=true in javascript
                  //       // 2- handshaking and then start of websocket (Here a connection:Keep-alive,Upgrade and upgrade:WebSocket headers are sent using http protocol and 101 response status code returned (Protocol Changed)) (so we use the pipeline once in the handshake)
                  //       Console.WriteLine($"Name:{endpoint.DisplayName};Route: {(endpoint as RouteEndpoint)?.RoutePattern}; Metadata: {String.Join(", ", endpoint.Metadata)}");
                  //   }
                  //   Console.WriteLine(ctx.Request.Headers["Authorization"]);
                  //   foreach (var item in ctx.Request.Cookies)
                  //   {
                  //       Console.WriteLine(@"{0}:{1}", item.Key, item.Value); // This does not work in Socket. Headers and Cookies (setting or getting) does not support in wss. but we can send them using special messages and callbacks and then in the client/server do the changes as we intended.
                  //                                                            // But since the socket needs handshake and handshake is a http(s) which request upgrade to ws(s) this may be reachable in handshake altough it open some security vulnarabilities. So try not use them
                  //   }

                  //   foreach (var item in ctx.Request.Headers)
                  //   {
                  //       Console.WriteLine(@"{0}:{1}", item.Key, item.Value);
                  //   }

                  await next(ctx);

                  foreach (var item in ctx.Response.Headers)
                  {
                      Console.WriteLine(@"{0}:{1}", item.Key, item.Value);
                  }
              });



            app.UseCors(builder =>
            {
                builder.WithHeaders("https://localhost:5010")
                .WithMethods("POST", "GET");
            });//The protections provided by CORS don't apply to WebSockets. This cors policy apply to handshaking which is done using Http protocol!!
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // The SignalR server library is included in the ASP.NET Core shared framework. 
                endpoints.MapHub<ChatHub>("/chathub", options =>
                {
                    // Her we configure Server Options related to HttpConnection Dispatcher Here (and related to Hub option and json/msgpack serializer in service registeration) : 
                    // ***Options Related to Authorization
                    // The Authorize properties added to hub which can be manually added here too
                    options.AuthorizationData.Add(new AuthorizeAttribute()); // this usually load from Authorize properties defined for hub
                    // options.CloseOnAuthenticationExpiration=true; // seems that removed (in case of web socket if Authorization expires it expires too)


                    // Buffer Limits
                    // SignalR uses per-connection buffers to manage incoming and outgoing messages. 
                    // instead of sending stream directly from producer to consumer, there is a buffer in one of them (usually producer) and try to make it full and then flush.
                    // for data sent from client to server and the buffer is on Application
                    options.ApplicationMaxBufferSize = 64 * 8 * 1024; //64 KB by defaut
                    // for data send from server to client and the buffer is on application again
                    options.TransportMaxBufferSize = 64 * 8 * 1024; //64KB

                    // Transports (choose between WebSocket, SSO and long Pooling as bit Flag enum as Transport Layer)
                    options.Transports = HttpTransportType.WebSockets | HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling; // By default all enabled wich equals 7

                    // Setting Regard to long polling
                    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(90); // (90seconds by default) The maximum amount of time the server waits for a message to send to the client before terminating a single poll request. 

                    // setting Regard Web Socket
                    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(5); // (5 seconds by default) After the server closes, if the client fails to close within this time interval, the connection is terminated (switch from gracefull close to ungracefull close).

                });// This url should be different than address of chat page. This is only an endpoint which uses concept of Hub

                endpoints.MapHub<StreamingHub>("/streaming");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
