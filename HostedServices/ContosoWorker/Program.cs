using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ContosoWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            // var monitorLoop = host.Services.GetRequiredService<MonitorLoop>();
            // monitorLoop.StartMonitorLoop();
            host.Run();
            // CreateHostBuilder(args).RunConsoleAsync(); // we can use this 
            // CreateHostBuilder(args).Start();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // This is for scheduled works
                    services.AddHostedService<TimedHostedService>();
                    // // This is for scoped DI
                    // services.AddHostedService<MyServiceHostedService>();
                    // services.AddScoped<IScopedProcessingService, ScopedProcessingService>();
                    // this is for Queuing
                    // services.AddSingleton<IBackgroundTaskQueue>((ctx) =>
                    // {
                    //     if (int.TryParse(hostContext.Configuration["QueueCapacity"], out int capacity))
                    //     {
                    //         return new BackgroundTaskQueue(capacity);
                    //     }
                    //     return new BackgroundTaskQueue(100);
                    // }); // Add Queue implementation with capacity of 100
                    // services.AddSingleton<MonitorLoop>();// services for capturing console and simulating each work item
                    // services.AddHostedService<QueuedHostedService>();
                });
    }
}


// In ASP.NET Core, background tasks can be implemented as hosted services.
// A hosted service is a class with background task logic that implements the IHostedService interface
// As an example for Background task consider:
// Background task that runs on a timer (like scheduled job in Windows)
// Hosted service that activates a scoped service. The scoped service can use dependency injection (DI). for example consider a task which clean expired cach item from the cache
// To use scoped services within a BackgroundService, create a scope. No scope is created for a hosted service by default.
// Queued background tasks that run sequentially.

// The ASP.NET Core Worker Service template provides a starting point for writing long running service apps.
// dotnet new worker 
// This app uses Microsoft.Net.SDK.Worker as sdk and including Hosting Infrastructure to configure services needed

// The IHostedService interface defines two methods for objects that are managed by the host:
// StartAsync(CancellationToken)
// StopAsync(CancellationToken)

// StartAsync
// StartAsync contains the logic to start the background task. StartAsync is called before:
//     The app's request processing pipeline is configured.
//     The server is started and IApplicationLifetime.ApplicationStarted is triggered.

// To change the default behaviour and Run StartAsync After them, you can add Hosted Service itself as a service after creating the builder:
// var builder=ContosoWorker.Program.CreateHostBuilder(args);
// builder.Services.AddHostedService<MyHostedService>();

// StopAsync
// StopAsync(CancellationToken) is triggered when the host is performing a graceful shutdown. (Process can disconnect connection and end background task safely)
// StopAsync contains the logic to end the background task.
// Implement IDisposable and finalizers (destructors) to dispose of any unmanaged resources.
// The cancellation token has a default five second timeout to indicate that the shutdown process should no longer be graceful 

// The hosted service is activated once at app startup and gracefully shut down at app shutdown. If an error is thrown during background task execution, Dispose should be called even if StopAsync isn't called.

// IHostedService is an interface. One of its implementations are the BackGroundService Abstract Base class for implementing a long running IHostedService.
// This class has an abstract method ExecuteAsync which returns a task. the service has one thread and assigned to this method unless we use the asynchronous method for example using async-await. 
// ExecuteAsync(CancellationToken) is called to run the background service. The implementation returns a Task that represents the entire lifetime of the background service. 
// StartAsync should be limited to short running tasks because hosted services are run sequentially, and no further services are started until StartAsync runs to completion.
// Long running tasks should be placed in ExecuteAsync.


// An example of using Hosted Service
// Consider we have a task which needs to do a heavy query on database (for example top 10 of most selled products). If we include it in
// The app itself, it run on every request which is not good option since it is heavy. We can add a hosted service here, which runs on the same server as app but we
// can run it in a timed manner in a way that we have the result updated for example every 10 min or something like this. 
// Of course we have other options like caching here instead of hosted service.
// There is two option similar to hosted service including Worker Service and Windows Service. Windows Service needs a separate machine than app but Hosted service run in same machine as web app but as hosted service. 
// Worker service on ther other hand run in a context different than web app and can be shared between multiple web app as a singleton service.