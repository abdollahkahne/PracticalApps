using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorWebAssemblySignalRApp.Client;
using Microsoft.Extensions.Configuration.Memory;
using BlazorWebAssemblySignalRApp.Shared;
using BlazorWebAssemblySignalRApp.Client.Pages.ExternalEvents;
using BlazorWebAssemblySignalRApp.Client.Pages;
using ComponentLibrary;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

var builder = WebAssemblyHostBuilder.CreateDefault(args); // Here we create the default host builder for WebAssembly
// The following to line should be deleted for blazor wasm prerendering since we completely delete the index.html and add index.cshtml!
// builder.RootComponents.Add<App>("#app");
// builder.RootComponents.Add<HeadOutlet>("head::after");
// To register a component to work with in JS
builder.RootComponents.RegisterForJavaScript<Dialog>("dialog");

// To add a component to a css selector in razor page/view (This does not happen in Prerendering! and if we have other pages which do not have this css selector and uses blazor components in some way browser crashes! Do not USE this approach except for app)
// builder.RootComponents.Add<Counter>(".counter-from-css");

var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }; // better to use http client factory here? No. since it runs on client it do not  encounter problems related to server and http handler I think

// create http client using SocketsHttpHandler
// var handler = new SocketsHttpHandler();
// handler.PooledConnectionLifetime = TimeSpan.FromMinutes(2);
// builder.Services.AddScoped(sp => new HttpClient(handler, disposeHandler: false)); // 'SocketsHttpHandler' is unsupported on: 'browser'
builder.Services.AddScoped(sp => http);

// builder.Services.AddRemoteAuthentication()
builder.Services.AddSingleton<AuthenticationStateProvider>(new CustomAuthenticationStateProvider());

// builder.Services.AddOptions(); // I think this added by default to default web assembly host builder
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("NothAuthorized", policy =>
    {
        policy.RequireAssertion(ctx =>
        {
            var resource = ctx.Resource;
            // If we need to access to resource object in runtime and its methods use
            // if (ctx.Resource is String str) {// so we have access to string methods}
            var username = ctx.User.Identity!.Name;
            if (username == resource.ToString())
            {
                return true;
            }
            return false;
        });
    });
});

builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<ISettingService, SettingService>();

builder.Services.AddSingleton<NotifierService>(); // This two service used as an example of an external service which runs in Browser!
builder.Services.AddSingleton<TimeService>(); // This and the above services should be singleton (and in this order since Notifier service injected at Timer) since we should have one event handling system which we can add event handler to it and raise it externaly

// this should be added to Services in order to use it inside ComponentLibrary or outside it.
builder.Services.AddScoped<ExampleJsInterop>();

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
