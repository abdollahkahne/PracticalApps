Authentication in Asp.Net Core
Authentication is the process of determining a user's identity while Authorization is the process of determining whether a user has access to a resource. Authentication is responsible for providing the ClaimsPrincipal for authorization to make permission decisions against.
Autentication done in authentication middleware by using IAuthenticationService. Authentication Service uses Authentication Handlers (AuthenticationHandlers) registered in configuration to do Authentication-Related actions (Authenticating User, Forbiding Requests, Challanging user and also SignIn/SignOut user). The authentication handler with their configuration (which has a name too) called authentication scheme. To register these handlers/schemes we should first create an instance of AuthenticationBuilder (by registering Authentication service in service collection using services.AddAuthentication()) and then use its AddScheme method to register the schemes in Authentication Builder. We have lots of other extension methods which add different schemes to authentication builder (like AddCookie and AddJwtBearer). We can register multiple schemes but we should specify the scheme we want to use when we do Authentication-Related actions (This actions sometime may invoke by authorization process: forbid or challange). If default scheme does not specify, framework throw an error (We can also specify the default scheme for each action in AddAuthentication method). In some cases like using ASP.NET Core Identity for authentication, the AddAuthentication done internally when registering them. 
To summarize following, AuthenticationService is responsible for Authentication related actions. This service has methods for each action (authenticate,challange,forbid,signIn,signOut) which when we call them, it calls related method from handler of registered default scheme. Every scheme instantiate once in AddScheme method (and so every authentication handler) but every handler Initialized for each request to cover HttpContext values. As already said result of Authentication process authenticate method is a claims principal.

AuthenticationHandler
An authentication handler:
    Is a type that implements the behavior of a scheme.
    Is derived from IAuthenticationHandler or AuthenticationHandler<TOptions>.
    Has the primary responsibility to authenticate users.
Based on the authentication scheme's configuration and the incoming request context, authentication handlers:
    Construct AuthenticationTicket objects representing the user's identity if authentication is successful.
    Return 'no result' or 'failure' if authentication is unsuccessful.
    Have methods for challenge and forbid actions for when users attempt to access resources:
        They are unauthorized to access (forbid).
        When they are unauthenticated (challenge).
An specific type of AuthenticationHandlers are RemoteAuthenticationHandler<TOptions>. That is the class for authentication that requires a remote authentication step. When the remote authentication step is finished, the handler calls back to the CallbackPath (a path in base address) set by the handler. The handler finishes the authentication step using the information passed to the callback path. OAuth 2.0 and OIDC both use this pattern (The third party responsible for Authentication called Identity/Authentication Provider. Also Google and facebook and other social network may provide an OpendID Connetc Providers for remote authentication. If it only do authentication it is OAuth 2.0 and if it manage access to some resources they are OIDC). JWT (use request authorization header directly) and cookies (use request cookie directly) do not. 
As it already said every Authentication Handler should be able to handle Authenticate, Forbid or Challange actions. These actions are done per request and also there is too other action (SignIn and SignOut which is done per session/user/connection for example ). But what these actions are:
Authenticate action is responsible for constructing the user's identity based on request context. Authenticate examples include:
    A cookie authentication scheme constructing the user's identity from cookies.
    A JWT bearer scheme deserializing and validating a JWT bearer token to construct the user's identity.
An authentication challenge is invoked by Authorization when an unauthenticated user requests an endpoint that requires authentication. Authentication challenge examples include:
    A cookie authentication scheme redirecting the user to a login page.
    A JWT bearer scheme returning a 401 result with a www-authenticate: bearer header.
A challenge action should let the user know what authentication mechanism to use to access the requested resource.
An authentication scheme's forbid action is called by Authorization when an authenticated user attempts to access a resource they are not permitted to access. Authentication forbid examples include:
    A cookie authentication scheme redirecting the user to a page indicating access was forbidden.
    A JWT bearer scheme returning a 403 result.
    A custom authentication scheme redirecting to a page where the user can request access to the resource.

An authentication scheme is a name which corresponds to:
    An authentication handler.
    Options for configuring that specific instance of the handler.

Related Types and Classes
IAuthenticationService
As already said this is what is used by middleware to do authentication actions. This interface has following five methods:
* AuthenticateAsync(HttpContext, String): Authenticate current request for the specified authentication scheme and return asychronousely an instance of AuthenticationResult
* ChallengeAsync(HttpContext, String, AuthenticationProperties): Challenge request according the specified authentication scheme and authentication properties. This is usually called by developer inside controller action or by other services in framework like Authorization services.
* ForbidAsync(HttpContext, String, AuthenticationProperties): Forbids request according the the specified authentication scheme and authentication properties specified.
* SignInAsync(HttpContext, String, ClaimsPrincipal, AuthenticationProperties): Sign a principal in for the specified authentication scheme. For example in case of cookie authentication it create a cookie which can be used by AuthenticateAsync method
* SignOutAsync(HttpContext, String, AuthenticationProperties): Sign out the specified authentication scheme. For example in case of cookie authentication it clears related cookies.
Extension method for IAuthentication services are:
* GetTokenAsync(HttpContext, String, String): In case of working with authentication tokens (Name/Value representing a token) there is an extension method which authenticate request according authentication scheme and returns token value for a specified token.

Note: To make it easy to call these method for an special scheme, some extension method developed on HttpContext. for example we have context.AuthenticateAsync(string schemeName) which Authenticate the current request using the specified scheme.

AuthenticationBuilder
An instance of this class create using service.AddAuthentication() and then Used to configure authentication. This method has a main method for adding and configuring an authentication scheme and lots of extension methods for configuration of different authentication schemes like cookie, jwt, oauth, OIDC
* AddScheme<TOptions,THandler>(String, Action<TOptions>) where THandler is an authentication handler and TOptions are scheme options.
* AddRemoteScheme<TOptions,THandler>(String, String, Action<TOptions>): used for THandler which inherit from RemoteAuthenticationHandler and has three arguments schemeName, schemeDisplayName, scheme configuration options. 
And some of its extension methods are (this methods has multiple overload which can receive schemeName, schemeDisplayName and schemeOptions and here only the one with two arguments showed):
* AddCookie(String, Action<CookieAuthenticationOptions>): Cookie authentication uses a HTTP cookie persisted in the client to perform authentication. 
* AddJwtBearer(String, Action<JwtBearerOptions>): JWT bearer authentication performs authentication by extracting and validating a JWT token from the Authorization request header. 
* AddOAuth(String, Action<OAuthOptions>)
* AddOpenIdConnect(String, Action<OpenIdConnectOptions>): OpenID Connect is an identity layer on top of the OAuth 2.0 protocol. It allows clients to request and receive information about authenticated sessions and end-users. 
* AddGoogle()
* AddFacebook()
* AddTwitter()
* AddMicrosoftAccount()
* AddIdentityCookies(Action<IdentityCookiesBuilder>)
* AddApplicationCookie()

We here only consider describing Cookie-based and jwt authentications and AddGoogle() method as a remote/OAuth2.0 based method.
As already said an instance of AuthenticationBuilder returned as a result of AddAuthentication method. This method has following overloads:
* AddAuthentication()
* AddAuthentication(string): It gets default authentication scheme
* AddAuthentication(Action<AuthenticationOptions>): It configures authentication options for example default authentication for each authentication-related actions.

AuthenticationScheme
AuthenticationSchemes assign a name to a specific IAuthenticationHandler handlerType. Altough Scheme Configuration is considered the difference of registered Scheme and handler (when we register scheme) but it is not a property of AuthenticationScheme. This scheme configuration is related to AuthenticationOptions and AuthenticationService instance (schemes or _schemes field). Authentication scheme itself has 3 properties including DisplayName(string), Name(string), HandlerType(Type) with constructor new AuthenticationScheme(String, String, Type). This constructor used internally by AuthenticationBuilder's AddScheme methods. AuthenticationScheme has not any method. AuthenticationScheme instantiate once when registeration with TOption options but AuthenticationHandler for the scheme InitializeAsync in every request (Altough it is instantiate once!)

IAuthenticationHandler
An object with this interface Created per request by Authentication service to handle authentication for a particular scheme (If we want to be precise, an instance of class THandler:AuthenticateHandle<TOptions> are created). IAuthenticationHandler has methods for each authentication-related actions methods in AuthenticationServices. For example when we called AuthenticationService.AuthenticateAsync(HttpContext context, string schemeName) it calls HandleAuthenticateAsync/AuthenticateAsync Method in Handler for scheme of scheme name. It has following methods:
* InitializeAsync(AuthenticationScheme scheme,HttpContext context): Initialize the authentication handler (not instantiate!). The handler should initialize anything it needs from the request and scheme as part of this method. The Scheme Options (TOptions) considered in the instance of scheme when registeration/ AddScheme method.
* AuthenticateAsync(): this method authenticate current request and returns an AuthenticateResult instance which has an AuthenticationTicket Field which has A ClaimsPrincipal field. 
* ChallengeAsync(AuthenticationProperties? properties): Challenge the current request.
* ForbidAsync(AuthenticationProperties? properties): Forbid the current request.

AuthenticationHandler<TOptions>:IAuthenticationHandler
This class is An opinionated abstraction for implementing IAuthenticationHandler. This type implemented to create THandler for an scheme. TOptions here is a type which extends AuthenticationSchemeOptions and should have a parameterless constructor. As you can see later, this Type is base class for multipl type of handlers directly or indirectly. JwtBearerHandler is one of derived handlers. All of the handler extends this handler should have a constructor new AuthenticationHandler<TOptions>(IOptionsMonitor<TOptions>, ILoggerFactory, UrlEncoder, ISystemClock)(). This is what which used when instantiation of the handler.
It implemented methods from interface:
* InitializeAsync(AuthenticationScheme, HttpContext)
* AuthenticateAsync()
* ChallengeAsync(AuthenticationProperties)
* ForbidAsync(AuthenticationProperties)
And it has addition methods (not all of them):
* HandleAuthenticateAsync(): This method Allows derived types to handle authentication.
* HandleChallengeAsync(AuthenticationProperties): Override this method to deal with 401 challenge concerns, if an authentication scheme in question deals an authentication interaction as part of it's request flow. (like adding a response header, or changing the 401 result to 302 of a login page or external sign-in location.)
* HandleForbiddenAsync(AuthenticationProperties): Override this method to handle Forbid.
* HandleAuthenticateOnceAsync(): Used to ensure HandleAuthenticateAsync is only invoked once. The subsequent calls will return the same authenticate result.
* CreateEventsAsync(): Creates a new instance of the events instance.
* InitializeEventsAsync(): Initializes the events object, called once per request by InitializeAsync(AuthenticationScheme, HttpContext).
* InitializeHandlerAsync(): Called after options/events have been initialized for the handler to finish initializing itself.
It has properties like:
* Events (object?): The handler calls methods on the events which give the application control at certain points where processing is occurring. If it is not provided a default instance is supplied which does nothing when the methods are called.

SignOutAuthenticationHandler<TOptions>:AuthenticationHandler<TOptions>
This is an extension to AuthenticationHandler<TOptions> which has 2 added methods to Adds support for SignOutAsync.
* SignOutAsync(AuthenticationProperties)
* HandleSignOutAsync(AuthenticationProperties)

SignInAuthenticationHandler<TOptions>:SignOutAuthenticationHandler<TOptions>
This is an extension to SignOutAuthenticationHandler<TOptions> which has 2 added methods to Adds support for SignInAsync.
* SignInAsync(ClaimsPrincipal, AuthenticationProperties)
* HandleSignInAsync(ClaimsPrincipal, AuthenticationProperties): Override this method to handle SignIn in derived class.

RemoteAuthenticationHandler<TOptions>:AuthenticationHandler<TOptions>
An opinionated abstraction for an AuthenticationHandler<TOptions> that performs authentication using a separately hosted provider. OAuthHandler<TOption> and OpenIDConnectHandler are derived from this type. The difference between this handler and AuthenticationHandler<TOptions> are on implementing the methods and some Properties.
Note: As you can see there is two method for each authentication-related actions for example AuthenticateAsyn and HandleAuthenticateAsync. If we want to override the behaviour in our custom handler we should override the HandleAuthenticateAsync.

AuthenticationOptions
This type is Options to configure authentication. This is used in AddAuthentication method when adding authentication to service container. It has following Properties:
* string? DefaultScheme: Used as the fallback default scheme for all the other defaults.
* string? DefaultAuthenticateScheme: Used as the default scheme by AuthenticateAsync in AuthenticationService.
* string? DefaultChallengeScheme 
* string? DefaultForbidScheme
* string? DefaultSignInScheme
* string? DefaultSignOutScheme
* IDictionary<String,AuthenticationScheme>  SchemeMap {get;}
* IEnumerable<AuthenticationSchemeBuilder> Schemes {get;}
And it has following method which add schemes directly:
* AddScheme<THandler>(String, String)

CookieAuthenticationOptions
We present this type to know what fields/properties a TOptions type have in implementing Authentication Handler. This is options used in CookieAuthenticationHandler:
* PathString AccessDeniedPath: The AccessDeniedPath property is used by the handler for the redirection target when handling ForbidAsync.
* CookieBuilder Cookie: the settings used to create the cookie. This settings are cookie Name, samesite, HttpOnly and expiration/ExpireTimeSpan.
* TimeSpan ExpireTimeSpan
* bool SlidingExpiration
* string ReturnUrlParameter: default is "returnUrl" used in login Page to redirect after successful login.
* PathString LoginPath 
* PathString LogoutPath
* CookieAuthenticationEvents Events (https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.cookies.cookieauthenticationevents?view=aspnetcore-6.0)

AuthenticationProperties
Dictionary used to store state values about the authentication session. It has following constructor:
new AuthenticationProperties(IDictionary<String,String>). This properties used in some Authentication-Related actions. For example it can be passed to SignInAsync method.
The important property of it is but it has other properties too:
*  bool IsPersistent: whether the authentication session is persisted across multiple requests.

AuthenticationTicket
Contains user identity information as well as additional authentication state. It has following constructors:
* new AuthenticationTicket(ClaimsPrincipal principal, String schema)
* new AuthenticationTicket(ClaimsPrincipal principal, AuthenticationProperties properties, String schema)
It has following Properties:
* ClaimsPrincipal Principal
* string AuthenticationScheme
* AuthenticationProperties Properties

AuthenticationResult
As it said, the return type of AuthenticateAsync is AuthenticationResult. It has following Properties:
* AuthenticationTicket Ticket
* AuthenticationProperties Properties
* ClaimsPrincipal Principal
* bool Succeeded: If a ticket was produced, authenticate was successful.
* bool None: Indicates that there was no information returned for this authentication scheme
* Exception? Failure (Holds failure information from the authentication.)
It has following static methods:
* Success(AuthenticationTicket ticket): Indicates that authentication was successful
* Fail(Exception): Indicates that there was a failure during authentication.
* NoResult(): Indicates that there was no information returned for this authentication scheme

The following example is HandleAuthenticateAsync:
....
var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
return Task.FromResult(AuthenticateResult.Success(ticket));

