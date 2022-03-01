JS interop
A Blazor app can invoke JavaScript (JS) functions from .NET methods and .NET methods from JS functions. These scenarios are called JavaScript interoperability (JS interop).

Note: If DOM is managed by Blazor do not change it from JS. If an element rendered by Blazor is modified externally using JS directly or via JS Interop, the DOM may no longer match Blazor's internal representation, which can result in undefined behavior.
Note: JS interop calls are asynchronous by default, regardless of whether the called code is synchronous or asynchronous.

JavaScript initializers
JavaScript (JS) initializers execute logic before and after a Blazor app loads.  JS initializers are useful in the following scenarios:
    Customizing how a Blazor app loads.
    Initializing libraries before Blazor starts up.
    Configuring Blazor settings.
JS initializers are detected as part of the build process and imported automatically in Blazor apps. To define a JS initializer, add a JS module to the project named {NAME}.lib.module.js, where the {NAME} placeholder is the assembly name, library name, or package identifier (It can be used for RCLs too). Place the file in the project's web root, which is typically the wwwroot folder (in both RCL and Projects).
it can export following methods:
1- beforeStart(options, extensions): Called before Blazor starts. For example, beforeStart is used to customize the loading process, logging level, and other options specific to the hosting model. 
2- afterStarted(blazor): Called after Blazor is ready to receive calls from JS. For example, afterStarted is used to initialize libraries by making JS interop calls and registering custom elements. The Blazor instance is passed to afterStarted as an argument

Note: If we used a razor component in MVC and Razor Pages apps, they don't automatically load JS initializers

JS Can be added from following Places:
1- Added to head secttion (Note Recommended)
2- Add to body of Index.html or _host.cshtml
3- Add to collocated js file and load them when required
4- Load an script from external file
5- inject script after blazor started.

Note: Don't place a <script> tag in a Razor component file (.razor) because the <script> tag can't be updated dynamically by Blazor (since script needs parsing with js parser!).

Add Script to Head
It can be added to Head of html in index.html/_layout.cshtml/_host.cshtml, but it is not recommended since:
1- It takes some time to parse it and so the page load is slower
2- since Blazor object is not ready yet we can not do much except the function and object definition.
3- It pollutes global object (window)

Add Script to end of Body
since it pollute the global object, it better to do start and main initialization there. For example in case of manual start of Blazor we can use end of Body to place it.

Collocated JS File
As we display it previously, they can be added with same name (including extension) to the same place as page, view or component and in publish/build time they added to wwwroot folder with the full path (~/pages/index.razor.js). So we should consider this full path for loading and using them. 
In case of Blazor, we can use them by importing it using import function in OnAfterRenderAsync(bool fistRender) method as below (Where JS is an instance of IJSRuntime service injected in component):
module = await JS.InvokeAsync<IJSObjectReference>(
    "import", "./Pages/Index.razor.js");

We can use collocated javascript file in RCL too. the only difference is to use them we should use the path as /_content/RCLPackageID/.. in both components in RCL and component placed in main App:
var module = await JS.InvokeAsync<IJSObjectReference>("import", 
    "./_content/AppJS/Pages/Index.razor.js");

Global external JS Files
They should be place in wwwroot in App itself or RCLs and they should reference similary. To Add them we can add them to end of body after Blazor script or we can use them directly same to collocated js files. 

Load after Blazor Start
If we manually start Blazor using the following script it return a js promise which after it resolution we can call any js script we want. 
Also we have Initializers afterStarted which do the above for automatic start. 

Cache JS and static Assets
JavaScript (JS) files and other static assets aren't generally cached on clients during development in the Development environment. During development, static asset requests include the Cache-Control header with a value of no-cache or max-age with a value of zero (0).
During production in the Production environment, JS files are usually cached by clients.

Call JS Function from dotnet
As already saw, you can use InvokeAsync<T> and InvokeVoidAsync (There is an sxtended method with this name with same functionality too which exist in JSRuntimeExtensions and JSObjectReferenceExtensions) from dotnet to call a js function. The name of js function here is (the function identifier (String)) is relative to the global scope (window). To call window.someScope.someFunction, the identifier is someScope.someFunction. Of course in case of using IJSObjectReference extension method for js module, we can use the name relative to module.
There is multiple signature/overload for it and some of its inputs (aside from function identifier) are:
1- an Object[] which represents js function arguments and can get any number of JSON-serializable arguments.
2- The cancellation token (CancellationToken) propagates a notification that operations should be canceled (same as abort function in js I think!).
3- TimeSpan represents a time limit for a JS operation.

The output of method for InvokeAsync<T>: The TValue return type must also be JSON serializable. TValue should match the .NET type that best maps to the JSON type returned (Could we define other return type except IJSStreamReference or IJSObjectReference?? Yes it is true if it be JSON Serializable!).
If js function returns a promise<T>, the await itself wait for js promise to resolve too. 

Note: One of use case of JS Interop is to use browser APIs from .NET. there is lots of APIs developed for browser which may be useful in some scenarios. To see the list of browser API you can see:
https://developer.mozilla.org/en-US/docs/Web/API

Note: Some browser API, due to security consideration, can only be triggered by user and can not triggered in code (API restericted to user gesture). For example consider the case the api for making a video playback in full screen. To do them we should trigger them as an event raised by user (For example onclick). But in case of Blazor Server since it send to server through another event which is not raised by user (signalR), the method do not execute if we use @onclick which handle by Blazor. In this case you should handle the method in js and using onclick trigger totally in JavaScript context. 

Note: For Blazor Server apps with prerendering enabled, calling into JS isn't possible during initial prerendering. JS interop calls must be deferred until after the connection with the browser is established. We get the following error in case of using JS Interop on Serverside:
JavaScript interop calls cannot be issued during server-side prerendering, because the page has not yet loaded in the browser. Prerendered components must wrap any JavaScript interop calls in conditional logic to ensure those interop calls are not attempted during prerendering.
Basically do not do any js/dom manipulation untill afterRender called. 

Use JS Interop in class
We used JS Interoperability in Razor component above. but if we want to use them in all c sharp class we can do same. Inject IJSRuntime and use it. So we should use Microsoft.JSInterop namespace here. 
Then we can use it by instantiation in OnInit life step. We can not use Inject Service if we do not register them in Service collection, but we can register it if we want to use it on multiple places.
What happen if we call a method that need browser to execute (For example do UI change) in Blazor Server?! No I do not think it works!
JavaScript interop calls cannot be issued during server-side prerendering, because the page has not yet loaded in the browser. Prerendered components must wrap any JavaScript interop calls in conditional logic to ensure those interop calls are not attempted during prerendering.







