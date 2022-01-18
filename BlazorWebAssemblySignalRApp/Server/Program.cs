using BlazorWebAssemblySignalRApp.Server.Hubs;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;

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

// A Content Type is how the server tells the browser what type of file the resource being served is.  
// That way the browser knows how to render whether it’s HTML, CSS, JSON, PDF, etc.  
// The way the server does this is by passing a Content-Type HTTP Header. And it is important to set this content-type (even some times changing it to correct type in browser does not work as expected) 
// A Content Type can also be called a MIME type, but the header is called Content-Type, and ASP.NET Core calls it the Content Type 
// for a vast majority of the static files you’re going to serve, the Static Files Middleware will set the Content Type for you.  
// For scenarios where you need to set the Content Type yourself:
// 1- In Controller and Page Handler we can use File helper method for FileResult as return File(fileContent, "application/pdf");
// 2- In Static File Middleware use following pattern (We need to use this middleware two times!)
var provider = new FileExtensionContentTypeProvider();
provider.Mappings.Add("srt", "text/plain");

var staticFilesOptions = new StaticFileOptions
{
    ContentTypeProvider = provider,
};// We can use static file options to add file extensions related to content-type for example
app.UseStaticFiles(staticFilesOptions);
app.UseStaticFiles(); // For other File types

app.UseRouting();

app.MapHub<ChatHub>("/chathub");
app.MapRazorPages();
app.MapControllers();

// In hosted Blazor WebAssembly apps that aren't prerendered, pass StaticFileOptions to MapFallbackToFile that specifies response headers at the OnPrepareResponse stage.
var staticFileOptions = new StaticFileOptions()
{
    OnPrepareResponse = (ctx) => ctx.Context.Response.Headers.Add("sampleHeader", "my sample header"),
};


app.MapFallbackToFile("index.html", staticFileOptions); // This is necessary for blazor to work. And it should has an id #App which connected to blazor wasm

app.Run();

// The -ho|--hosted option creates a hosted Blazor WebAssembly solution (dotnet new blazorwasm -ho -o MyBlazorWasm).
// It creates three project:
// 1- A MVC/Razor Web App for using as backend
// 2- A client which is blazor web assembly and sent to browser and hast the blazor wasm services and also an http client to server
// 3- A Shared Class Library Project for interface, classess and Services which is common betweem these two



