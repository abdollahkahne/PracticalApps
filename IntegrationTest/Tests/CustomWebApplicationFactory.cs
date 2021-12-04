using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Src.Data;

namespace Tests
{
    // This class is resposible for creation a test host and test server which used for integration tests and used to Bootstrap the SUT (Subject Under Test)
    // This class also give us one or more HttpClients which sends http requests messages to our test server
    // We should use the startup class responsible for creating the main project (Sometimes Program class may be used but here we used for StartUp class)
    // This class parameterless constructor which can be inherited from WebApplicationFactory<T> if not specified strictly,
    // try to determin content root of startup project and load all the dependencies assembly from its content root
    // This class has interesting method which can useful from learning prespective  of Generic Hosts, Web Hosts and Web Server. I displayed some of them at the end of this class
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        // we use this method to do more configuration to web host
        // For Generic hosts:this method execute before application built but after configure services for example or after ConfigureWebBuilder()
        // For Web Host: we should use builder.ConfigureTestServices() which executed after confiure services otherwise our change does not applied
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // builder.ConfigureTestServices(); // always execute after app startup configure service
            builder.ConfigureServices(services =>
            {
                //remove main database and change it with in Memory test database
                // builder.ConfigureServices callback is executed after the app's Startup.ConfigureServices code is executed. so our changes remains
                var dbContextOptionsServiceDescriptor = services.SingleOrDefault(sd => sd.ServiceType == typeof(DbContextOptions<AppDbContext>));
                services.Remove(dbContextOptionsServiceDescriptor); // service descriptor is very usefull class which defines a service from three view point: 1- Its Lifetime (Transient,Scoped,Singleton) 2- Interface (Service) Type 3- Implementation Type (Implementation Instance)
                // For example we can implement a singleton service as simple as new ServiceDescriptor(IMyService,MyService,ServiceLifetime.Singleton)
                // var sd = new ServiceDescriptor(typeof(IGithubClient), typeof(GithubClient), ServiceLifetime.Singleton);
                // services.Add(sd);
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("InMemoryDbForTesting"));

                // Another approachs to get Scope and get service provider are:
                // 1-Directly use service provider exist in base class
                // Services.CreateScope();
                // 2- Build Host and use its service provider
                // var host=builder.Build();
                // var sp=host.Services;
                // 3- Build Service Provider from IServiceCollection 

                var serviceProvider = services.BuildServiceProvider();
                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServicesProvider = scope.ServiceProvider;
                    var dbContext = scopedServicesProvider.GetRequiredService<AppDbContext>();
                    var dbCreated = dbContext.Database.EnsureCreated();

                    if (dbCreated)
                    {
                        var logger = scopedServicesProvider.GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();
                        try
                        {
                            // Intialized database
                            dbContext.Initialize();
                        }
                        catch (System.Exception ex)
                        {

                            logger.LogError(ex, "An Error Occured During seeding database with test messages. Error: {Message} ", ex.Message);
                        }
                    }

                }
            });
            // builder.UseEnvironment("production");
        }

    }

}


// Method and Properties of Web Application Factory:
// 1- Services: this is a service provider which can be used to get required services.
// 2- Server: This is the test server by this factory using CreateServer method and can be used to get Features, Services and Host 
// (Host is responsible for cutting concern problem like life management, Middleware components, Configuration and Server implementation, dependency injection, logging and Services)
// (Server is responsible for listening to request and surface them to the app as http context)
// 3- CreateHostBuilder()/CreateWebHostBuilder() The default implementation of this method looks for a public static IHostBuilder CreateHostBuilder(string[] args) method defined on the entry point of the assembly
// 4- WithWebHostBuilder(Action<IWebHostBuilder> Config): More configuration on host builder itself
// 5- CreateHost(IHostBuilder builder) use to create host with defined HostBuilder
// 6- ConfigureWebHost(IWebHostBuilder builder): Gives a fixture (TextFixture that uses this factory) an opportunity to configure the application before it gets built
// 7- CreateServer(IWebHostBuilder builder): This method is for compatability usage in case of using WebHostBuilder instead of HostBuilder. The server created indirectly in CreateHost() method in Generic hosts. So try to get server from property if it is required
// 8- CreateClient(): This method is responsible for creating a client that handle redirecting and cookie management. We use this to send the requests
// 9- ConfigureClient(HttpClient client):  we can do more configuration to client here


// ** Important****
// The test app's builder.ConfigureServices callback is executed after the app's Startup.ConfigureServices code is executed.
// The execution order is a breaking change for the Generic Host with the release of ASP.NET Core 3.0.
// For SUTs that still use the Web Host, the test app's builder.ConfigureServices callback is executed 
// before the SUT's Startup.ConfigureServices code. The test app's builder.ConfigureTestServices callback is executed after.
