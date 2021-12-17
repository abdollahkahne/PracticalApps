using ClientSide;
using SignalRClient.Hubs;

var builder = WebApplication.CreateBuilder(args);
// builder.Host.ConfigureWebHost(option => option.UseUrls("https://localhost:5002"));// this do not work here but we can change the launch setting

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<HubClient>();
builder.Services.AddSingleton<ClockHubClient>();

var app = builder.Build();

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();
