using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using GoogleBlazor.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// builder.Services.AddRazorPages();
// builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme
            ).AddCookie()
            .AddGoogle(options =>
            {

                options.ClientId = "475003216877-jnobm8or2j121jne0h285fl2td0e48sc.apps.googleusercontent.com";
                options.ClientSecret = "FV9X2JM0c9PTxpPBt6KfnNdk";
                options.SignInScheme = "Cookies"; // This is the schema that used for persisting user info for this case in cookie. 
                // options.SaveTokens = true;
                options.Events.OnTicketReceived = ctx =>
                {

                    return Task.CompletedTask;
                };
            });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id?}");

// app.MapBlazorHub();
// app.MapFallbackToPage("/_Host");

app.Run();
