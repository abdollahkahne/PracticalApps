using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using BlazorServerSignalRApp.Data;
using Microsoft.AspNetCore.ResponseCompression;
using BlazorServerSignalRApp.Hubs;
using BlazorServerSignalRApp.CircuitHandlers;
using Microsoft.AspNetCore.Components.Server.Circuits;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(); // This add Blazor Server Side AND it seems that it add SignalR by itself. We can Add SignalR Hub Options here 
// We need using System for all of this
// builder.services.AddRazorPages().AddHubOptions(options=>{}); // with this we can configure HubOption 
// builder.services.AddServerSideBlazor(options=>{}); // with this we can set circuit option which is specific to Blazor Hub like JSInteropDefaultCallTimeout which is by default 1 minute
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }); // When we want to add an entry to enumerable we can use concat as here (Apparanyly append works similary too but here we can not use append since our initial value is option.MimeType which is empty)
});

// Register a Circuit Handler
builder.Services.AddSingleton<CircuitHandler, TrackingCircuitHandler>();

var app = builder.Build();

//Add response compression first of all
app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub(); // This is what used by SignalR to handle event handling from client side to server and also connection at the first place to blazor hub to create a live connection for that and apply updates to UI
app.MapHub<ChatHub>("/chathub"); // The fallback route should be at the end always
app.MapFallbackToPage("/_Host"); // This is what handle the first calling of the pages

app.Run();
