using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using BlazorServerSignalRApp.Data;
using Microsoft.AspNetCore.ResponseCompression;
using BlazorServerSignalRApp.Hubs;
using BlazorServerSignalRApp.CircuitHandlers;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using static BlazorServerSignalRApp.Data.SettingService;
using System.Security.Claims;
using SharpPad;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(); // This add Blazor Server Side AND it seems that it add SignalR by itself. We can Add SignalR Hub Options here 
// We need using System for all of this
// builder.services.AddServerSideBlazor().AddHubOptions(options=>{}); // with this we can configure HubOption 
// builder.services.AddServerSideBlazor(options=>{}); // with this we can set circuit option which is specific to Blazor Hub like JSInteropDefaultCallTimeout which is by default 1 minute
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }); // When we want to add an entry to enumerable we can use concat as here (Apparanyly append works similary too but here we can not use append since our initial value is option.MimeType which is empty)
});

// Register a Circuit Handler
builder.Services.AddSingleton<CircuitHandler, TrackingCircuitHandler>();

// Add HttpClient Factory (To inject we should inject HttpClientFactory and then create a new http client)
builder.Services.AddHttpClient();

builder.Services.AddControllers(); // enable api

// Enable Open ID Connect as Authentication Scheme
// builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
// {
//     options.Scope.Add("openid");
//     options.SaveTokens = true;
//     options.ResponseType = OpenIdConnectResponseType.Code;
// });
builder.Services.AddScoped<TokenProvider>();
// Console.WriteLine(builder.Configuration.GetSection("GoogleOpenID").Value);
// var client = new HttpClient();
// var res = await client.GetAsync("https://googleapis.com/oauth2/v3/certs");
// // var keys = await res.Content.ReadFromJsonAsync<string>(new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
// var keys = await res.Content.ReadAsStringAsync();
var GoogleCertsExpected = new JsonWebKeySet();


GoogleCertsExpected.Keys.Add(
     new JsonWebKey
     {
         Alg = "RS256",
         E = "AQAB",
         Kty = "RSA",
         Kid = "58b429662db0786f2efefe13c1eb12a28dc442d0",
         N = "xszAZmDzaUa5d8anZL6ZExj0YNiUVZqzFxWQGKT3fPw5N5lKb_eVtxFKFjgyfOx8Lm1NbVIFVBNTGFsd42MMSU-CrEMMsWe3WTgSzwCmW4t5XE__y1b7MkUTd4WkSzgifMok_SD4D8x-Gd1-awC6nLu0bEbqLWcaXtwfogDiO2nMTgQcuVBGH3ZA-sS7ASgNK-3bNM0mXeVvaIzRPAahZ9tzJ_CEj8mrDVdmgSsO42PTnYtfQc1nytLwNX19_92HQAvWLtQ3-zjZ0FlJUGFTUui8whktgRXv2eXyp-bNkprD7HORUjzCU0Ugwq-nfa1zyYrBDpwQ8FVnS6opUK7iAQ",
         Use = "sig",
     });
GoogleCertsExpected.Keys.Add(
     new JsonWebKey
     {
         Alg = "RS256",
         E = "AQAB",
         Kty = "RSA",
         Kid = "cec13debf4b96479683736205082466c14797bd0",
         N = "1YWUM8Y5UExSfXsBrF6oACI48nITxDf07CiYKn_VTbLRlpXX1AfNtQhrjm-jPjC16qXnGCBhdlZHdCycfezoMg8svo41U7YIVLP5G5H6f7VxAEglmV5IGc0kj35__qmqy3t1Eug_iqxCOyRlcDELQ75MNOhYFQtjeEtLuw4ErpPpOeYVX71vOH3Q9epItMM0n18FXW5Dd6BkCiHvMkb5eSHOH07J0h-MkRF133R-YSPPgDlqLeRxdjDo2rwqKFsOa68edzconVcETWR2YSoFtangVd-IBhzFrax8gyVsntKpmbg8XyJZU2vtgMiTdP0wAjAe8gy78Dg1WIOVOe58lQ",
         Use = "sig",
     });


var configurationGoogle = new OpenIdConnectConfiguration()
{
    Issuer = "https://accounts.google.com",
    AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth",
    TokenEndpoint = "https://oauth2.googleapis.com/token",
    UserInfoEndpoint = "https://openidconnect.googleapis.com/v1/userinfo",
    JwksUri = "https://www.googleapis.com/oauth2/v3/certs",
    EndSessionEndpoint = "https://oauth2.googleapis.com/revoke",
    // EndSessionEndpoint = "https://accounts.google.com/o/oauth2/revoke",
};
var openIDEvents = new OpenIdConnectEvents()
{
    // 1- Redirect To Identity Provider: Here we can add state query string to url for double check of signature
    // Getting Endpoints from metadata endpoint already done. I am not sure where it is done?
    // 2- OnMessageReceived: This runs on first response retrieved from Identity Provider (which include Code too)
    // 3-  OnAuthorizeCodeReceived Executes
    // 4- Token Response Received: This is done in exchange of code we received in previous http request
    // 5- Token Validate: After Receiving the token (which is access token and )
    // This is the last event
    OnTicketReceived = ctx =>
    {
        // Console.WriteLine("OnTicketReceived Executed");
        // Console.WriteLine(@"Name in Ticket: {0}", ctx.Result.Ticket.Principal.Identity.Name); // does not set automaticly
        List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList();
        foreach (var item in tokens)
        {
            // Console.WriteLine(@"{0}:{1}", item.Name, item.Value);
        }
        return Task.CompletedTask;
    },
    OnAuthorizationCodeReceived = ctx =>
    {
        // Console.WriteLine(@"Authorization EndPoint: {0}", ctx.Options?.Configuration?.AuthorizationEndpoint);
        // Console.WriteLine("OnAuthorizationCodeReceived Executed");
        // Console.WriteLine(@"Grant Type: {0}", ctx.TokenEndpointRequest.GrantType);
        return Task.CompletedTask;
    },
    OnMessageReceived = async messageReceivedContext =>
    {
        var options = messageReceivedContext.Options;
        if (options.TokenValidationParameters.ValidIssuer == null || options.TokenValidationParameters.IssuerSigningKeys == null)
        {
            var credentials = await options.Backchannel.GetFromJsonAsync<GoogleKeys>(options.Configuration!.JwksUri, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, });
            // var keySet = new JsonWebKeySet();
            // foreach (var item in credentials!.Keys)
            // {
            //     keySet.Keys.Add(item);
            // }
            options.TokenValidationParameters.IssuerSigningKeys = credentials?.Keys;
            options.TokenValidationParameters.ValidIssuer = options.Authority;
        }
        // Console.WriteLine(@"Code in MessageReceived Step: {0}", messageReceivedContext.ProtocolMessage.Code);
        // Console.WriteLine(@"ID Token in MessageReceived Step: {0}", messageReceivedContext.ProtocolMessage.IdToken); // It is return in case of ResponseType.CodeI
        // Console.WriteLine(@"Access Token in MessageReceived Step: {0}", messageReceivedContext.Token);
        // Console.WriteLine("OnMessageReceived Executed");
    },

    OnRedirectToIdentityProvider = ctx =>
    {
        // Console.WriteLine(@"Authorization EndPoint: {0}", ctx.ProtocolMessage.AuthorizationEndpoint);
        // Console.WriteLine("OnRedirectToIdentityProvider Executed");
        return Task.CompletedTask;
    },
    OnTokenResponseReceived = ctx =>
    {
        // Console.WriteLine(@"ID Token: {0}", ctx.TokenEndpointResponse.IdToken);
        // Console.WriteLine("OnTokenResponseReceived Executed");
        return Task.CompletedTask;
    },
    // The following should be run after receiving ticket and 
    OnTokenValidated = ctx =>
    {
        // Console.WriteLine("OnTokenValidated Executed");
        return Task.CompletedTask;
    },
    OnUserInformationReceived = ctx =>
    {
        var user = ctx.User.Deserialize<ClaimsPrincipal>(new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, });
        // Console.WriteLine("OnUserInformationReceived Executed");
        if (ctx?.Principal?.Claims is not null)
        {
            foreach (var item in ctx.Principal.Claims)
            {
                // Console.WriteLine(@"Claim {0}: {1} ({2})", item.Type, item.Value, item.ValueType);
            }
        }
        return Task.CompletedTask;
    },
    OnAuthenticationFailed = ctx =>
    {
        // Console.WriteLine("OnAuthenticationFailed Executed");
        return Task.CompletedTask;
    },
    // OnRedirectToIdentityProviderForSignOut = async ctx =>
    // {
    //     var options = ctx.Options;
    //     // var req = new HttpRequestMessage();
    //     // var formData = new List<KeyValuePair<string, string>>();
    //     // formData.Add(new KeyValuePair<string, string>("token", ctx.ProtocolMessage.IdToken));
    //     // var content = new FormUrlEncodedContent(formData);
    //     // var res = await options.Backchannel.PostAsync(options.Configuration?.EndSessionEndpoint, content);
    //     var res = await options.Backchannel.GetAsync($"{options.Configuration.EndSessionEndpoint}?token={ctx.ProtocolMessage.AccessToken}");
    //     // res.EnsureSuccessStatusCode();
    //     Console.WriteLine(res.Content);
    //     ctx.HandleResponse();
    // }
};

// builder.Services.AddAuthentication(
//             //     options =>
//             // {
//             //     options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//             // options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//             //     options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//             //     options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//             // }
//             ).AddCookie()
//             .AddGoogle(options =>
//             {
//                 // In OAuth authentication there is following step usually
//                 // 1- In Browser user redirect to The Authorization EndPoint (Client ID used here) of provider and login
//                 // 2-Provider redirect to the CallBackUrl with a temporary code 
//                 // 3- Server process the request and send a request to The Token EndPoint (Client Secret and temporary Code parameter used here) and return a Token and Some basic user info here!
//                 // 4- (Only in Oauth and not in OpenIDConnect???) server use the token to request further user information and make further request to the provider resources
//                 // All of the setting can be set here
//                 // options.AuthorizationEndpoint="https://github.com/login/oauth/authorize";// This can be set in the hyperlink manually to but we can use Challange to go to this link
//                 // options.TokenEndpoint="https://github.com/login/oauth/access_token"; This used by Authentication Middleware in step 3
//                 // options.UserInformationEndpoint="https://api.github.com/user";// not called by the OAuth authentication handler itself, but instead is something we need to call to obtain more information about the user

//                 // For doing Authentication we need to create a claim principal with at least two claim type:Name,NameIdentifier. The response which we receive in step 3 or 4 has this claim but it needs to map them
//                 // options.ClaimActions.MapJsonKey(ClaimTypes.Name, "related json key from response");
//                 // options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
//                 // options.ClaimActions.MapJsonKey("urn:github:login", "login");
//                 // options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
//                 // options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");

//                 // // Now we should feed the above claim actions with a json. This json returned as a result to step 4 (Or may be 3). So we should hook to them
//                 // options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents()
//                 // {
//                 //     // This is the step 4 and not done automataicly
//                 //     OnCreatingTicket = async (ctx) =>
//                 //     {
//                 //         // make the request 
//                 //         var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
//                 //         request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
//                 //         request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ctx.AccessToken);
//                 //         // send it and get the response
//                 //         var response = await ctx.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ctx.HttpContext.RequestAborted);
//                 //         response.EnsureSuccessStatusCode();
//                 //         // feed the response to claim actions
//                 //         var user = JObject.Parse(await response.Content.ReadAsStringAsync()); // JObject is part of Newton Json
//                 //         ctx.RunClaimActions(user); // the user used in claim action defined above
//                 //     },
//                 // };



//                 options.ClientId = "475003216877-jnobm8or2j121jne0h285fl2td0e48sc.apps.googleusercontent.com";
//                 options.ClientSecret = "FV9X2JM0c9PTxpPBt6KfnNdk";
//                 options.SignInScheme = "Cookies"; // This is the schema that used for persisting user info for this case in cookie. 
//                 // options.CallbackPath = "/Account/GoogleResponse";
//                 // options.CorrelationCookie = new Microsoft.AspNetCore.Http.CookieBuilder { SecurePolicy = CookieSecurePolicy.None };

//                 // Here we can create a mapping between Claims Provided by Google and our defined claim
//                 // We should save them if we want to make the persist over multiple request
//                 options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
//                 options.ClaimActions.MapJsonKey("urn:google:locale", "locale", "string");
//                 options.ClaimActions.MapJsonKey("urn:google:user:birthday", "birthday", "string");

//                 // SaveTokens is set to false by default to reduce the size of the final authentication cookie (Important to keep cookie size small. Claims are also save in cookie, so we should have as small as needed)
//                 options.SaveTokens = true; // this save the token in cookie.  we can retrieve the access token by calling the GetTokenAsync() method
//                 // The Events include multiple events: Here we used the OnCreatingTicket which Invoked after the provider successfully authenticates a user
//                 options.Events.OnCreatingTicket = ctx =>
//                 {
//                     List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList();

//                     tokens.Add(new AuthenticationToken()
//                     {
//                         Name = "TicketCreated",
//                         Value = DateTime.UtcNow.ToString()
//                     });

//                     ctx.Properties.StoreTokens(tokens);

//                     return Task.CompletedTask;
//                 };
//                 // Add more scope
//                 options.Scope.Add("https://www.googleapis.com/auth/user.birthday.read");
//             });

builder.Services.AddAuthentication(options =>
{
    // options.DefaultSignInScheme = "Cookies";
    // options.DefaultAuthenticateScheme = "Cookies";
    // options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    options.DefaultScheme = "Cookies";
}).AddCookie()
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme,
    options =>
    {
        options.ClientId = "475003216877-jnobm8or2j121jne0h285fl2td0e48sc.apps.googleusercontent.com";
        options.ClientSecret = "FV9X2JM0c9PTxpPBt6KfnNdk";
        options.Configuration = configurationGoogle;
        // options.SignInScheme = "Cookies";
        options.CallbackPath = new PathString("/signin-google");
        // options.TokenValidationParameters = new TokenValidationParameters
        // {
        //     TryAllIssuerSigningKeys = true,
        //     ValidateIssuerSigningKey = true,
        //     // IssuerSigningKeys = GoogleCertsExpected.Keys,
        //     RequireExpirationTime = true,
        //     ValidateLifetime = true,
        //     RequireSignedTokens = true,
        // };
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("profile");
        options.Scope.Add("https://www.googleapis.com/auth/user.birthday.read");
        options.SaveTokens = true;
        options.Authority = "https://accounts.google.com";
        // options.MetadataAddress = "https://accounts.google.com/.well-known/openid-configuration";
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.Events = openIDEvents;
    }
); // we should implement a serice which do OpenIDConnect for us. The above configuration only enable it to save the data in cookie!


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
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
app.MapBlazorHub(); // This is what used by SignalR to handle event handling from client side to server and also connection at the first place to blazor hub to create a live connection for that and apply updates to UI
app.MapHub<ChatHub>("/chathub"); // The fallback route should be at the end always
app.MapFallbackToPage("/_Host"); // This is what handle the first calling of the pages
// This route named: low-priority route  or fallback route

app.Run();
