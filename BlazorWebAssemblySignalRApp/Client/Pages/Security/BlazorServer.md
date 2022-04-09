Authentication in Blazor Server
Blazor Server uses ASP.NET Core Identity and doesn't offer a separate authentication process within Blazor for authentication and authorization.
Since Blazor Server is based upon SignalR and SignalR uses WebSocket instead of Http we can not use authentication directly in websocket. Authentication in SignalR-based apps is handled when the connection is established (Negotiation/Post or Hub Initialization). Authentication can be based on a cookie or some other bearer token. The built-in AuthenticationStateProvider service for Blazor Server apps obtains authentication state data from ASP.NET Core's HttpContext.User. This is how authentication state integrates with existing ASP.NET Core authentication mechanisms. (So simply use an authentication mechanism in app and AuthenticationStateProvider has its value if the HttpContext.User has value after authentication step in Asp.Net (Which is true if SignIn do in and it is do in implemented handler already exists) )
Note: In a Blazor Server app, services for options and authorization are already present, so no further action is required. But in Web Assembly we may add them if it is necessary. 

It uses Asp.Net Core Identity using Razor Page and switch from/to http when it require to do an authentication action. Fundamental challenges exist to implementing security independent of ASP.NET Core Identity (Something in websocket instead of http layer!) for Blazor Server:
* ASP.NET Core Identity provides the UI layer using Razor Pages, which are designed to work in the context of a request-response model, contrary to Blazor, which works in a stateful model over a WebSocket connection.
* SignInManager<TUser>, UserManager<TUser>, and other Identity abstractions expect an available HTTP request and response to function properly.
* HTTP cookies and other implementations for authentication can't function over a WebSocket connection, which is a fundamental challenge to performing authentication in Blazor.
Due to this challanges and since the majority of SPA frameworks implement an authentication process where users are redirected to an external provider (for example google!) and returned to the app. In this regard, Blazor Server is similar to other SPA frameworks.

To implement a custom Authentication State Provider in Blazor you should extend AuthenticationStateProvider and override its Task<AuthenticationState> GetAuthenticationStateAsync() method to return an instance of AuthenticationState Asynchronously. AuthenticationState has a field User which is a ClaimsPrincipal.

Other Security Concerns in Blazor Server
Blazor Server apps adopt a stateful data processing model, where the server and client maintain a long-lived relationship (Logically). The persistent state is maintained by a circuit, which can span connections that are also potentially long-lived (Physically since it is ws). circuit invokes JavaScript functions in the user's browser and .NET methods on the server.
When a user visits a Blazor Server site, the server creates a circuit in the server's memory. The circuit indicates to the browser what content to render and responds to events, such as when the user selects a button in the UI. To perform these actions, a circuit invokes JavaScript functions in the user's browser and .NET methods on the server. This two-way JavaScript-based interaction is referred to as JavaScript interop (JS interop).
Note: Events in Blazor has two versions for example onclick and @onclick. When user work with onclick browser handle it completely independent (Unless you use Invoke from DotNet) but in case of @onclick, it uses JSInterop rules. In JS Interop when you call a js function you should use a .Net method for it (you can only assign .Net methods to @onclick). So when @onclick event araise first a signal message sent to server (circuit) to do the .Net method. It the method has a js function it send back the signal to invoke js in browser. Then in browser if it totally done in browser, it sends a signal to inform end of js invoke. An then server send back another signal to inform End Dotnet method. Every of these signal can have data too (when we use DotNetObjectReference, JSObjectReference or streams). Also if we call a Dotnet method inside JS, it do same instructions.

Because JS interop occurs over the Internet and the client/App/Server uses a remote browser (App in Client), Blazor Server apps share most web app security concerns.

Blazor Server and Shared State in Server side
Blazor server apps live in server memory. For each app session, Blazor starts a circuit with its own DI container scope. That means that scoped services are unique per Blazor session. We don't recommend apps on the same server share state using singleton services unless extreme care is taken, as this can introduce security vulnerabilities, such as leaking user state across circuits. You can use stateful singleton services in Blazor apps if they are specifically designed for it. For example, it's ok to use a memory cache as a singleton because it requires a key to access a given entry, assuming users don't have control of what cache keys are used.
Additionally, again for security reasons, you must not use IHttpContextAccessor within Blazor apps. Blazor apps run outside of the context of the ASP.NET Core pipeline. The HttpContext isn't guaranteed to be available within the IHttpContextAccessor, nor is it guaranteed to be holding the context that started the Blazor app.

Question: How to pass HttpContext or other similar request data to Blazor? 
Answer: At first request we use a Razor page (.cshtml) to call Blazor app. We can define a parameter for App component and pass all the required value to it in first load as below:
1- Define a class with all the data you want to pass to the Blazor app
2- Populate that data from the Razor page using the HttpContext available at that time.
3- Define a parameter in the root component to hold the data being passed to the app.
4- Pass the data to the Blazor app as a parameter to the root component (App).
5- Use the user-specific data within the app; or alternatively, copy that data into a scoped service within OnInitializedAsync so that it can be used across the app.
Note: App.razor Initialization Happen once, since it only render once!

Resource Exhaustion in Blazor Server
Resource exhaustion can occur when a client interacts with the server and causes the server to consume excessive resources. Excessive resource consumption primarily affects:
* CPU
* Memory
* Client Connection
Resources external to the Blazor framework, such as databases and file handles (used to read and write files), may also experience resource exhaustion.

Note: CPU exhaustion is a concern for all public-facing apps. In regular web apps, requests and connections time out as a safeguard, but Blazor Server apps don't provide the same safeguards (Timeout for request/connection). We should try to implement Timeouts in other scopes since the context is different in socket and we should not close request/connection in each communication message.

CPU: CPU exhaustion is a concern for all public-facing apps. In regular web apps, requests and connections time out as a safeguard, but Blazor Server apps don't provide the same safeguards. Blazor Server apps must include appropriate checks and limits before performing potentially CPU-intensive work.
Memory:  As a general recommendation, don't permit clients to allocate an unbound amount of memory as in any other server-side app that persists client connections. The memory consumed by a Blazor Server app persists for a longer time than a single request. Some tips are:
    * Use paginated lists when rendering.
    * Only display the first 100 to 1,000 items and require the user to enter search criteria to find items beyond 
    * implement lists or grids that support virtualization. Using virtualization, lists only render a subset of items currently visible to the user. When the user interacts with the scrollbar in the UI, the component renders only those items required for display.
Connection: Connection exhaustion can occur when one or more clients open too many concurrent connections to the server, preventing other clients from establishing new connections. Blazor clients establish a single connection per session and keep the connection open for as long as the browser window is open. By default, there's no limit on the number of connections per user for a Blazor Server app. If the app requires a connection limit, take one or more of the following approaches:
    * Require authentication
    * Set Limit on number of connection per user (This is possible at app level and server level using proxy and also keep track of active session per user)

Denial of service (DoS) attacks
Denial of service (DoS) attacks involve a client causing the server to exhaust one or more of its resources making the app unavailable. Blazor Server apps include default limits which can be set at Circuit Options or Hub Options:
* CircuitOptions.JSInteropDefaultCallTimeout (indicates how long the server will wait before timing out an asynchronous JavaScript function invocation. TimeSpan and Default to 1 min)
* CircuitOptions.DisconnectedCircuitMaxRetained (etermines the maximum number of disconnected circuit state details are retained by the server. Default is 100)
* CircuitOptions.DisconnectedCircuitRetentionPeriod ( determines the maximum duration state for a disconnected circuit is retained on the server. Default is 3 min TimeSpan)
* CircuitOptions.MaxBufferedUnacknowledgedRenderBatches (maximum number of render batches that a circuit will buffer until an acknowledgement for the batch is received. Default is 10)
* HubConnectionContextOptions.MaximumReceiveMessageSize (the maximum message size the client can send. Default is 32KB equal to 32*1024)

Interaction Between browser and Server in Blazor server
A client interacts with the server through JS interop event dispatching (@onclick instead onclick) and render completion. JS interop communication goes both ways between JavaScript (Browser) and .NET (Server):
    Browser events are dispatched from the client to the server in an asynchronous fashion.
    The server responds asynchronously rerendering the UI as necessary.
We have also invocation of JS functions from .Net methods and vice verse. 
Invocation of js function from .Net methods we should consider followings:
* All invocations have a configurable timeout after which they fail, returning a OperationCanceledException to the caller. This timeout may set by CircuitOptions.JSInteropDefaultCallTimeout (default is one minute) or we can use CancellationToken per call basis. 
* The result of a JavaScript call can't be trusted. The result may be an error or unexpected result. Take the following precautions to guard against the preceding scenarios:
    * Wrap JS interop calls within try-catch statements
    * Validate data returned from JS interop invocations
Also in case invoking exposed .Net method from JS, Don't trust calls from JavaScript to .NET methods and Treat it like public endpoint to the App by:
    * Validating Inputs (Ensure inputs are in the expected range and also user has necessary permission)
    * Don't allocate an excessive quantity of resources as part of the .NET method invocation
    * Avoid sharing state across sessions (For example always use scoped sevices both in static and instance .Net methods)
    * To prevent XSS vulnerability, do not pass user-supplied data to JS functions
In case of Events, Events provide an entry point to a Blazor Server app. The same rules for safeguarding endpoints in web apps apply to event handling in Blazor Server apps. We should consider follownigs:
    * The app must validate the data for any event that the app handles.
    * Blazor Server events are asynchronous, so multiple events can be dispatched to the server before the app has time to react by producing a new render. Limiting client actions in the app must be performed inside event handlers and not depend on the current rendered view state. For example instead of conditionally rendering a button, you should also consider conditionally handling the event (based on the current app state and not based on the state of the UI).
    * Gaurd against multiple dispatch in case of costly and long running operations such as fetching data from an external service or database (For example by setting/checking a flag before fetching)
    * Cancel Long running events in case of component disposal. This can be achived by using a cancellation token which generated from CancellationTokenSource in component disposal!
    * Avoid using events that generate large amount of data like OnScroll or OnInput
Additional Security Guidance
* Exposing error information to clients on the Internet is a security risk that should always be avoided.
* Blazor Server doesn't ensure the integrity and confidentiality of the data sent between the server and the client. Always use HTTPS.
* Cross-site scripting (XSS) allows an unauthorized party to execute arbitrary logic in the context of the browser. XSS attacks occur when an attacker uses a web application to send malicious code, generally in the form of a browser side script, to a different end user. For a XSS vulnerability to exist, the app must incorporate user input in the rendered page. Consider that xss does not require script always since it also can execute by other tags too, for example: <img src="http://url.to.file.which/not.exist" onerror=alert(document.cookie);> or <b onmouseover=alert('Wufff!')>click me!</b> or <body onload=alert('test1')>. With this vulnerability, attacker can perform malicious attack using all Different Dispatcher from browser to server like Dispatch fake/invalid events, dispatch fake/invalid render completion signals, dispatch unwanted interop calls to .Net methods from JS Or modify result of JS functions in response to .Net methods which invoke js functions.  It can also avoid the dispatchers requested (by User or by framework) like events, render completion signals or .net method calls using js-interop.
* Cross Origin protection include two type of measures: CORS (Cross origin Resource sharing) and CSRF (Cross site request forgery):
    * CORS: This means do not allow other sites/origin to send request to this origin. By default it is possible to send request to Blazr server application from other web sites which can be disabled for example by using .RequireCors("sample") at the end of MapBlazorHub.
    * CSRF: This is an attack that forces an end user to execute unwanted actions on a web application in which they’re currently authenticated. It inherits the identity and privileges of the victim to perform an undesired function on the victim’s behalf. For most sites, browser requests automatically include any credentials associated with the site, such as the user’s session cookie, IP address, Windows domain credentials, and so forth. Therefore, if the user is currently authenticated to the site, the site will have no way to distinguish between the forged request sent by the victim and a legitimate request sent by the victim. It is altouh done in other site but do in the context of origin so all data included with request. Fortunately, this request will not be executed by modern web browsers thanks to same-origin policy restrictions unless you use a CORS Policy with Access-Control-Allow-Origin: * (Which it is not possible in case of usine credentials fortunately!) But it can be mimics in origin site using XSS attacks.
* Click-jacking: Click-jacking involves rendering a site as an <iframe> inside a site from a different origin in order to trick the user into performing actions on the site under attack. This can be used, for example, to steal login credentials or to get the user's unwitting permission to install a piece of malware. To forbidden this, we can add a header to response as X-Frame-Options header or adding Content-Security-Policy: frame-ancestors 'self' https://www.example.org; header.
* Open Redirects: A malicious user can force the browser to go to an attacker-controlled site. In this scenario, the attacker tricks the app into using some user input as part of the invocation of the NavigationManager.NavigateTo method (Which handles it as an OnLocationChanged Event). Components must:
    Avoid using user input as part of the navigation call arguments.
    Validate arguments to ensure that the target is allowed by the app.
This advice also applies when rendering links as part of the app:
    If possible, use relative links.
    Validate that absolute link destinations are valid before including them in a page.

Security checklist
The following list of security considerations isn't comprehensive:
    Validate arguments from events.
    Validate inputs and results from JS interop calls.
    Avoid using (or validate beforehand) user input for .NET to JS interop calls.
    Prevent the client from allocating an unbound amount of memory.
    Guard against multiple dispatches.
    Cancel long-running operations when the component is disposed.
    Avoid events that produce large amounts of data.
    Avoid using user input as part of calls to NavigationManager.NavigateTo and validate user input for URLs against a set of allowed origins first if unavoidable.
    Don't make authorization decisions based on the state of the UI but only from component state.
    Consider using Content Security Policy (CSP) to protect against XSS attacks.
    Consider using CSP and X-Frame-Options to protect against click-jacking.
    Ensure CORS settings are appropriate when enabling CORS or explicitly disable CORS for Blazor apps.
    Test to ensure that the server-side limits for the Blazor app provide an acceptable user experience without unacceptable levels of risk.


Question: How a .razor component converted to DOM and What measures takes to prevent XSS? 
Answer: Blazor Server components execute a compile-time step where the markup in a .razor file is transformed into procedural C# logic. At runtime, the C# logic builds a render tree describing the elements, text, and child components. This is applied to the browser's DOM via a sequence of JavaScript instructions (or is serialized to HTML in the case of prerendering)
    * Razor Syntax used at .razor component (for example @userInput):  Razor syntax is added to the DOM via commands that can only write text. Even if the value includes HTML markup, the value is displayed as static text. When prerendering, the output is HTML-encoded, which also displays the content as static text. So as much as you use .razor component do not worry about User Input. 
    * Using script in component Render Tree: If a script tag is included in a component's markup, a compile-time error is generated.
    * Component with C# class without using Razor:  Use the correct APIs when emitting output. For example, use builder.AddContent(0, someUserSuppliedString) and not builder.AddMarkupContent(0, someUserSuppliedString)
