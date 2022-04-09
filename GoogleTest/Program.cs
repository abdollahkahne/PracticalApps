using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication("Cookies")
            .AddCookie().AddGoogle(options =>
            {
                options.ClientId = "475003216877-jnobm8or2j121jne0h285fl2td0e48sc.apps.googleusercontent.com";
                options.ClientSecret = "FV9X2JM0c9PTxpPBt6KfnNdk";
                options.SaveTokens = true;
                options.SignInScheme = "Cookies";

                // options.MetadataAddress = "https://accounts.google.com/.well-known/openid-configuration";
            });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
