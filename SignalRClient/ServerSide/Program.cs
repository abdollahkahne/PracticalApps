using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.SignalR;
using ServerSide.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR(options =>
{
    options.AddFilter<SetUserNameFilter>();
});
builder.Services.AddSingleton<SetUserNameFilter>();
builder.Services.AddSingleton<HttpContextAccessor>();
// Just added to have the user name in ChatHub
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("SignalR", options => { options.ClaimsIssuer = "SignalR"; });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.Use(async (context, next) =>
{
    Console.WriteLine(context.Request.Headers.Authorization);
    // Console.WriteLine(context.User.Identity.Name);
    await next(context);
});
app.UseAuthorization();
app.MapHub<ChatHub>("/chat");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
