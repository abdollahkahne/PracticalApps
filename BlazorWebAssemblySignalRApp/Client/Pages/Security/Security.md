Security in Blazor
Security scenarios differ between Blazor Server and Blazor WebAssembly apps. Because Blazor Server apps run on the server, authorization checks are able to:
    Determine The UI options presented to a user (for example, which menu entries are available to a user).
    Enforce Access rules for areas of the app and components.
Blazor WebAssembly apps run on the client. Authorization is only used to determine which UI options to show. Since client-side checks can be modified or bypassed by a user, a Blazor WebAssembly app can't enforce authorization access rules.

We can use one of the following approachs in component to get Authentication state from Authentication Service (This service may require Notifying Authentication State Change and handling the change in case of AuthenticationState Changes):
* Inject an instance of AuthenticationServiceProvider to component using @inject
* Create a cascading parameter of type Task<AuthenticationState> and get its parameter from CascadingAuthenticationState which is the recommended one. In case of Built-in components like AuthorizedView and AuthorizedRouteView use this approach (So we should add the Cascading Component in top level)

And to config authorization in components we should do one of the following (This require authentication congig to be done in components as above):
* Use Authorize attribute like ASP.Net Core in case of Paged Components (This required a AuthorizedRouteView in Routing and CascadingAuthenticationState). If we use it with RouteView instead, it ignore component and display it to all visitors. 
* Use built-in Component AuthorizedView to implement Role-Based, Policy-Based or Resource-Based Authorization. This gets Authentication State in component level. You can get the authentication state inside it using context (or you can rename it if you'd rather it by adding Context="varname" to component).
* Use Impretive approach using Authorization Service as we done in Asp.Net Core inside our custom components. The authentication state can be provide using the approachs described in Authentication.

Note: Razor Pages authorization conventions don't apply to routable Razor components. This convention includes Access Allow/ Deny to Page/Folder (These conventions allow you to authorize users and allow anonymous users to access individual pages or folders of pages.). one reason for this is that routing in Blazor handles by Router Component and not the Endpoint Middleware.
Note: AuthenticationStateProvider is the underlying service used by the AuthorizeView component and CascadingAuthenticationState component to get the authentication state. Note: AuthorizeRouteView colud use CascadingAuthenticationState internaly But also it add an outer tag to child component in case of Authorized user which makes the user's current authentication state available to descendants. 

Authentication in Blazor
Blazor uses the existing ASP.NET Core authentication mechanisms to establish the user's identity. The exact mechanism depends on how the Blazor app is hosted, Blazor WebAssembly or Blazor Server. There is a service AuthenticationStateProvider (in namespace Microsoft.AspNetCore.Components.Authorization) which is responsibe for authentication. To use this service in our Components in Blazor, there are components AuthorizeView and AuthorizeRouteView which use this service to add authorization to component parts or route  and also another component CascadingAuthenticationState which provide Authentication State to other components using cascading parameter pattern. We don't typically use AuthenticationStateProvider directly. It is problematic in cases where the underlying authentication state changed and we should notify other components. To use it, we should inject an instance of AuthenticationStateProvider registered in services.
Component CascadingAuthenticationState should be used arround all components which require authentication state. For example we can use it in App.razor around Router. It does not need any parameter. The component require authentication step can obtain the authentication state data by defining a cascading parameter of type Task<AuthenticationState>.

In Blazor WebAssembly apps, authentication checks can be bypassed because all client-side code can be modified by users (This is true for ALL Client side technologies). To use AuthenticationStateProvider service we should first add its nugget package to project. This service Provides information about the authentication state of the current user.

Blazor Server apps operate over a real-time connection that's created using SignalR. Authentication in SignalR-based apps is handled when the connection is established. Authentication can be based on a cookie or some bearer/jwt token.
The AuthenticationStateProvider in Blazor Server is built-in (ServerAuthenticationStateProvider) and provides authentication state from HttpContext.User (Created in Authentication Step outside of SignalR). So authentication state in Blazor server can be implemented using existing authentication mechanism in ASP.NET core. 
Altough Blazor server implements Identity for Authentication, SignInManager and UserManager are not available in Blazor. The reason are due to natural difference in Web Socket (Stateful Model over a connection) and Http (Request Based). These classes (managers) methods are with assumption that it can work with response and request to do tasks. 

Authorization in Blazor
After a user is authenticated, authorization rules are applied to control what the user can do.
Access is typically granted or denied based on whether:
    A user is authenticated (signed in). (Anynomous vs Authorized)
    A user is in a role. (Role based)
    A user has a claim. (Claim based)
    A policy is satisfied. (Policy- based)
We have other following rule too (Resource-Based)
    User has Access to the resource according to resource meta data (Resource-based) And as a variant of resource-based, user has access to view/page/component or other UI part depending in the Model used there, (View-based).
There is multiple use cases for authorization:
1- We want to show different content according to authorization conditions. In this case the approach is to use AuthorizeView Component. It has parameteres for specifying what content should be show to Authorized or Non-Authorized user and also what authorized user means there (By using Policy and Role)
2- You want to decide do you show a route/component to a user or not? In this case you can use one of the following Approach:
    * Use AuthorizeRouteView component:
    * Use Authorize Attribute in components: This can be used only in component with @page directive (And we should use default Blazor Router Component). If we use it in None Routed components, it does not evaluated. With this attribute we can implement role-based,claim-based or policy-based approach as other framework in ASP.Net.
3- You want to implement resource based authorization. In this approach you can define and register and AuthorizationHandler and then use that resource to decide user is authorized or not. Resource Parameter is available in both AuthorizeView and AuthorizeRouteView. To define an authorization handler you can implement related class or simply use Policy.RequireAssertion in policy builder (Read Authorization Requirement and Handler in Asp.Net Core).
4- You want to implement Authorization using Procedural logic (@code) in components (User Informations used for decision making and it is not only used for displaying!). In this case, To get AuthenticationState you better to use CascadingAuthenticationState and then define a cascading Parameter of type Task<AuthenticationState> and use it in your code. Other approach is to inject AuthenticationStateProvider and then work with it directly. Then You can call its GetAuthenticationStateAsync() method to get AuthenticationState. Buth disadvantage of using this approach is that when Authentication state changed (For example user signing out ), the event AuthenticationStateChanged may not raised or handled and the state be obslute. So using CascadingAuthenticationState is more reliable. We can use this authentication state to do all type of authorization.
* Default Policy: _awaitedState.User.Identity.IsAuthenticated
* RoleBased: _awaitedState.User.InRole("admin)
* Policy-Based and Resource-Based: await AuthorizationService.AuthorizeAsync(_awaitedState.User, "content-editor").Succeeded (require to @inject IAuthorizationService AuthorizationService)

Note: If you want to have access to an object methods and properties (with type object but other runtime type) you can use following approach:
if (obj is MyType item) {
    // Then item is in your type (MyType!) and can be worked with easily
}
Note: To work with System.Reflection you can add using to it and then @typeof(object) is runtime type which can be worked with.

Authorization And Authentication Classes/Types in Blazor
AuthenticationState
This Provides information about the currently authenticated user, if any. It is provided using an instance of AuthenticationStateProvider service. It has a property User (ClaimsPrincipal) and can be created using constructor new AuthenticationState(claimsPrincipal).
* ClaimsPrincipal User

AuthenticationStateProvider
This service Provides information about the authentication state of the current user. It is a type of Event system which has following Event:
* AuthenticationStateChanged (An event that provides notification when the AuthenticationState has changed. For example, this event may be raised if a user logs in or out.)
To raise this event we have following method:
* NotifyAuthenticationStateChanged(Task<AuthenticationState>) (Raises the AuthenticationStateChanged event)
And to get Authentication State it has following async method:
* GetAuthenticationStateAsync(): Asynchronously gets an AuthenticationState that describes the current user.
This Type inherited to ServerAuthenticationStateProvider and RemoteAuthenticationService types. 
This type is abstract and do not have constructor. So to use it we should implement and override GetAuthenticationStateAsync() method which is where our logic for authentication implemented.

Components used for Security
Following Components added by Microsoft.AspNetCore.Components.Authentication:
AuthorizeViewCore/AuthorizedView
AuthorizeViewCore component provide a way to show to authenticated and non-authorized users different content (display differing content depending on the user's authorization status). It has following Parameters:
1- ChildContent or Authorized: Content which showed to Authorized user (Only use one of them)
2- NotAuthorized: The content that will be displayed if the user is not authorized.
3- Authorizing: The content that will be displayed while asynchronous authorization is in progress.
4- Resource (object): The resource to which access is being controlled (???). For Resource-Based Policy definition/Authorization.
There is other derived component from AuthorizedViewCore named AuthorizedView which has 2 other parameter:
5- Policy (string): The policy name that determines whether the content can be displayed.
6- Roles (string): A comma delimited list of roles that are allowed to display the content.
This has also a method which should be overriden an return IAuthorizeData[] named GetAuthorizedData() and Gets the data used for authorization. IAuthorizeData has three fields Roles (string, comma separated list of Roles), Policy (string) and AuthenticationSchemes (string, comma separated list of schemas).
These components selectively displays UI content depending on whether the user is authorized. This approach is useful when you only need to display data for the user and don't need to use the user's identity in procedural logic. If we require to access to use info in this component we can use the Context field since its child componet is RenderFragment<AuthenticationState> (ChildContent is used if we do not use Authorized Parameter). For example:
<AuthorizeView>
    <h1>Hello, @context.User.Identity.Name!</h1>
    <p>You can only see this content if you're authenticated.</p>
</AuthorizeView>
Note: How context variable set here in the first place?!! It has the following private cascading parameter:
 [CascadingParameter] private Task<AuthenticationState>? AuthenticationState { get; set; }
and also another private property (AuthenticationState) currentAuthenticationState = await AuthenticationState ( it initialized at OnParameterSet);
And In BuildRenderTree it invokes RenderFragment<AuthenticationState> with this private property as below:
 builder.AddContent(0, authorized?.Invoke(currentAuthenticationState!)); (in .razor component you can use @authorized(currentAuthenticationState) to do same)

If authorization conditions aren't specified, AuthorizeView uses a default policy and treats:
    Authenticated (signed-in) users as authorized.
    Unauthenticated (signed-out) users as unauthorized.
Authorization Conditions here are Role, Policy and Resource.
For example:
<AuthorizeView Roles="admin, superuser">
    <p>You can only see this if you're an admin or superuser.</p>
</AuthorizeView>
<AuthorizeView Policy="content-editor">
    <p>You can only see this if you satisfy the "content-editor" policy.</p>
</AuthorizeView>
Claims-based authorization is a special case of policy-based authorization. For example, you can define a policy that requires users to have a certain claim.
Blazor allows for authentication state to be determined asynchronously. The primary scenario for this approach is in Blazor WebAssembly apps that make a request to an external endpoint for authentication (Client-side). In the meantime you can use Authorizing Parameter to show customized content to user. As it said this is not applicable to Blazor server or Prerendered version of Blazor wasm since it wait till completion of request and then send the response but it does not throw error if we use it. 
This component can be use to selectively displays content to users for Identity-related work (Show Login/Logout buttons or top bar that shows user info!). Also Altough this component can used in Nav and filter the menu but that is not prevent user to navigate manually to component route. For that case, you should use following AuthorizeRouteView component.

AuthorizeRouteView
This is RouteView component considering Authorization. RouteView is a component Displays the specified page component, rendering it inside its layout and any further nested layouts. We use it usually at App.razor as below where route data has 2 property PageComponent and RouteValues(every thing that can be specified in route template) and Default is the layout specified for rendering:
<RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
AuthorizeRouteView is derived from RouteView and has three addition for implementing Authorization:
* Resource (object): The resource to which access is being controlled. In case of AuthorizedRouteView usually we have only access to RouteData as Resource but of course you can define and add other Resource there. RouteData has RouteData.PageType and RouteData.RouteValues fields which can be used for decision making. RouteValues have route parameters defined in Route template. 
* Authorizing (RenderFragment): The content that will be displayed while asynchronous authorization is in progress.
* NotAuthorized (RenderFragment<AuthenticationState>): The content that will be displayed if the user is not authorized. 

CascadingAuthenticationState
This is disposable component which has no parameter instead ChildContent which is the components which have access to AuthenticationState.

UseFull Types and APIs in Security in ASP.Net Core

** System.Security.Principal
This namespace has lots of interface and classes but for web applications only the following is useful. 
IIdentity
IPrincipal
GenericIdentity
GenericPrincipal

** System.Security.Claims
This namespace Contains classes that implement claims-based identity in .NET, including classes that represent claims, claims-based identities, and claims-based principals. In this namespace we have 2 enumeration type classes named ClaimTypes and ClaimValueTpes and We have Claim, ClaimsIdentity and ClaimsPrincipal which is An IPrincipal implementation that supports multiple claims-based identities.  We have Claim-based vs Role-based access model which the first one implemented by this namespace. In the claims-based model, the IIdentity.Name property and the IPrincipal.IsInRole(String) method are implemented by evaluating the claims contained by an identity (NameClaimType and RoleClaimType). 

Claim
A claim is a statement about a subject by an issuer. Claims represent attributes of the subject that are useful in the context of authentication and authorization operations. Subjects and issuers are both entities that are part of an identity scenario. Some typical examples of a subject are: a user, an application or service, a device, or a computer. Some typical examples of an issuer are: the operating system, an application, a service, a role provider, an identity provider, or a federation provider. An issuer delivers claims by issuing security tokens (For example a google jwt has attributes fullname, email, picture). Sometimes subject itself has properties which indicate the claims. Its properties are:
* Type:string (from ClaimTypes Enumeration or a custom uri)
* Value:string
* ValueType:string (For complex types xml schema used as string)
* Subject:ClaimsIdentity (The subject is the entity about which the claim is asserted)
* Issuer:string (A name that refers to the issuer of the claim.)
It has multiple constructor but ususally this is used:
new Claim (string type, string value, string? valueType);

ClaimsIdentity:IIdentity
The ClaimsIdentity class is a concrete implementation of a claims-based identity; that is, an identity described by a collection of claims. In application code, ClaimsIdentity objects are typically accessed through ClaimsPrincipal objects;(for example, the principal returned by Thread.CurrentPrincipal in Windows or Context.User in Asp.Net). Its main constructor are ClaimsIdentity(IEnumerable<Claim>) and ClaimsIdentity(IEnumerable<Claim>, String) which Initializes a new instance of the ClaimsIdentity class with the specified claims and authentication type. Some of its important properties are:
* AuthenticationType:string (Gets the authentication type passed in constructor)
* Claims:IEnumerable<Claim> (The collection of claims associated with this claims identity from constructor)
* IsAuthenticated: bool (Gets a value that indicates whether the identity has been authenticated from Authentication State)
* Name:string (Returns the value of the first claim with a type that matches the name claim type set in the NameClaimType property.)
* NameClaimType: only if we use other than default claim type
* RoleClaimType
This class has methods which works on adding/removing/deleting/Has claims from Claims Property like:
* AddClaim(Claim)
* FindFirst(String),FindFirst(Predicate<Claim>),FindAll(string),FindAll(Predicate<Claim>)
* HasClaim(Predicate<Claim>), HasClaim(String, String)
* RemoveClaim(Claim), TryRemoveClaim(Claim)

ClaimsPrincipal:IPrincipal
ClaimsPrincipal exposes a collection of identities, each of which is a ClaimsIdentity (Usually a single one). As it already said, it can get from HttpContext.User or Thread.CurrentPrincipal. It has following constructor:
ClaimsPrincipal(IEnumerable<ClaimsIdentity>),ClaimsPrincipal(IIdentity),ClaimsPrincipal(IPrincipal),
Its main methods are: 
* Claims:IEnumerable<Claim>
* Identity:ClaimsIdentity
* Identites:IEnumerable<ClaimsIdentity>
Its main methods are:
* AddIdentity(ClaimsIdentity identity): The identity is added to the Identities collection.
* IsInRole(string role): Returns a value that indicates whether the entity (user) represented by this claims principal is in the specified role.
* HasClaim(Predicate<Claim>),HasClaim(String type, String value)

Context.User is an instance of this Type and If user.Identity.IsAuthenticated is true and because the user is a ClaimsPrincipal, claims can be enumerated and membership in roles evaluated.

ClaimTypes
Defines constants for the well-known claim types that can be assigned to a subject. This class cannot be inherited. These constants define URIs for well-known claim types.

ClaimValueTypes
Defines claim value types according to the type URIs defined by W3C and OASIS. This class cannot be inherited.

