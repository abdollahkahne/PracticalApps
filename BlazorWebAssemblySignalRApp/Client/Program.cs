using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorWebAssemblySignalRApp.Client;
using Microsoft.Extensions.Configuration.Memory;
using BlazorWebAssemblySignalRApp.Shared;
using BlazorWebAssemblySignalRApp.Client.Pages.ExternalEvents;
using BlazorWebAssemblySignalRApp.Client.Pages;

var builder = WebAssemblyHostBuilder.CreateDefault(args); // Here we create the default host builder for WebAssembly
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.RootComponents.RegisterForJavaScript<Dialog>("dialog");

var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }; // better to use http client factory here? No. since it runs on client it do not  encounter problems related to server and http handler I think
builder.Services.AddScoped(sp => http);

builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<ISettingService, SettingService>();

builder.Services.AddSingleton<NotifierService>(); // This two service used as an example of an external service which runs in Browser!
builder.Services.AddSingleton<TimeService>();

using (var response = await http.GetAsync("cars.json"))
{
    using (var stream = await response.Content.ReadAsStreamAsync())
    {
        builder.Configuration.AddJsonStream(stream); // Why we do not used builder.Configuration.AddJsonFile("cars.json");? 
    }
}

// builder.Configuration.AddJsonFile("/cars.json"); // This throw error since We are in browser sandbox so we have not permision to read files

// Add memory configuration provider
var vehicleData = new Dictionary<string, string>() {
    {"color","blue"},
    {"type","Cheetah"},
    {"wheels:count","4"},
    {"wheels:brand","Blazin"},
    {"wheels:brand:type","rally"},
    {"wheels:year","2008"}
};
var memoryConfig = new MemoryConfigurationSource { InitialData = vehicleData };
builder.Configuration.Add(memoryConfig);

// Use Configuration Binder for binding an option set with a section in configuration
builder.Services.AddOidcAuthentication(options => builder.Configuration.Bind("Local", options.ProviderOptions));

// Add configuration for logging (logging key in appsettings.json)
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// WebAssemblyHostConfiguration is saved in builder.Configuration and can be added by following methods like 
// Add, AddJsonFile,... and can be read using [] and GetSection and can be availabe to component from IConfiguration service injection
// Configuration are cached so in case of PWA they are not update until the next build and deploy of app by rebuilding 
// PWA's service-worker.js and service-worker-assets.js files 
foreach (var item in builder.Configuration.AsEnumerable())
{
    Console.Write(item.Key);
    Console.Write(":");
    Console.Write(item.Value);
    Console.WriteLine();
}


await builder.Build().RunAsync(); // If we need to have access to Registered service we can do Build first and then use them and at the end Run the host. This is good for test and initialization case like EF
