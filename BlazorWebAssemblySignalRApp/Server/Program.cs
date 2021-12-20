using BlazorWebAssemblySignalRApp.Server.Hubs;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR(); // Add signal R services here since it is not a blazor server and need that separately
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
});

var app = builder.Build();

app.UseResponseCompression();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapHub<ChatHub>("/chathub"); ;
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

// The -ho|--hosted option creates a hosted Blazor WebAssembly solution (dotnet new blazorwasm -ho -o MyBlazorWasm).
// It creates three project:
// 1- A MVC/Razor Web App for using as backend
// 2- A client which is blazor web assembly and sent to browser and hast the blazor wasm services and also an http client to server
// 3- A Shared Class Library Project for interface, classess and Services which is common betweem these two



