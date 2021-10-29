using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NorthwindCookieAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args) //Call CreateDefaultBuilder, which adds the default logging providers including console and debug and event in windows
                // .ConfigureLogging(builder =>{
                //     // builder.SetMinimumLevel(LogLevel.Error); // Generally, log levels should be specified in configuration and not code.
                //     builder.ClearProviders();
                //     builder.AddConsole(); // just for demonstration case
                //     // A filter function is invoked for all providers and categories that don't have rules assigned to them explicitly by configuration or code:
                //     // If it is return false, No log is applied
                //     builder.AddFilter((provider, category, logLevel)=>{
                //         if(provider.Contains("ConsoleLoggerProvider") && category.Contains("Controller") && logLevel>LogLevel.Trace) {
                //             return false;
                //         }
                //         return true;
                //     });
                // })
                // .ConfigureLogging((ctx,builder)=>{
                //     builder.AddConfiguration(ctx.Configuration.GetSection("logging"));
                // })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // webBuilder.ConfigureKestrel(options => options.ListenAnyIP(5002,listenOptions => {listenOptions.UseHttps();}));
                    // webBuilder.UseSetting("https_port","5002"); // seems wrong
                    // webBuilder.ConfigureKestrel(options =>{
                    //     options.ListenAnyIP(443,ops =>{
                    //         ops.UseHttps();
                    //     });
                    // });
                    webBuilder.UseKestrel(options =>options.AllowSynchronousIO=true); // This is needed for UseElm Middleware
                    webBuilder.UseStartup<Startup>();
                });
    }
}
