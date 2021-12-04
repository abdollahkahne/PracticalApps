using System;
using System.Threading.Tasks;
using APIClient.Services;
using Microsoft.Extensions.DependencyInjection;

namespace APIClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            configureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            try
            {
                await serviceProvider.GetRequiredService<HttpClientCrudService>().Execute();
                await serviceProvider.GetRequiredService<HttpClientStreamService>().Execute();
                await serviceProvider.GetRequiredService<HttpClientPatchService>().Execute();
                await serviceProvider.GetRequiredService<HttpClientCancellationService>().Execute();
                // var httpClients = serviceProvider.GetServices<IHttpClientServiceImplementation>();
                // foreach (var httpClient in httpClients)
                // {
                //     await httpClient.Execute();
                // }
            }
            catch (System.Exception ex)
            {

                Console.WriteLine($"Something went wrong: {ex}");
            }
        }

        private static void configureServices(IServiceCollection services)
        {

            // services.AddScoped<IHttpClientServiceImplementation, HttpClientCancellationService>();
            // services.AddScoped<IHttpClientServiceImplementation, HttpClientStreamService>();
            // services.AddScoped<IHttpClientServiceImplementation, HttpClientCrudService>();
            // services.AddScoped<IHttpClientServiceImplementation, HttpClientPatchService>();
            services.AddHttpClient<HttpClientStreamService>(options =>
             {
                 options.Timeout = TimeSpan.FromSeconds(35);
                 options.BaseAddress = new Uri("http://localhost:5000/api/");
                 options.DefaultRequestHeaders.Clear();
             });

            services.AddHttpClient<HttpClientCrudService>(options =>
            {
                options.Timeout = TimeSpan.FromSeconds(30);
                options.BaseAddress = new Uri("http://localhost:5000/api/");
                options.DefaultRequestHeaders.Clear();
            });
            services.AddHttpClient<HttpClientPatchService>(options =>
             {
                 options.Timeout = TimeSpan.FromSeconds(25);
                 options.BaseAddress = new Uri("http://localhost:5000/api/");
                 options.DefaultRequestHeaders.Clear();
             });

            services.AddHttpClient<HttpClientCancellationService>(options =>
            {
                options.Timeout = TimeSpan.FromSeconds(40);
                options.BaseAddress = new Uri("http://localhost:5000/api/");
                options.DefaultRequestHeaders.Clear();
            });

        }
        // private static void buildHost()
        // {
        //     // another approach to making service provider from service collection is to build host.
        //     //in that case we need Microsoft.Extension.Hosting too
        //     var host = new HostBuilder().ConfigureServices().Build(services => { services.AddScoped<>(); });
        //     // to get the service we use
        //     var myService = host.Services.GetRequiredService<MyService>();
        // }
    }
}
