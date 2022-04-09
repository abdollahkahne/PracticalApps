Http Client in Blazor
1- Http Client Factory in ASP
2- Calling an API in Blazor Server
3- Calling an API in Blazor Web Assembly
There is related topic with calling APIs like serialization,streams and of course Http Request, Response, Headers, HttpContent which may require reviewing.

Http Client Factory
The pattern used in ASP is to use HTTP Client Factory which injects and then we create the Http Client instance when it required. This has following benefit regard to inject Http Client directly:
1- Provides a central location for naming and configuring logical HttpClient instances. For example we create a HttpClient and configure it for GitHub Requests! Of course you can register a default one without naming for general purposes. 
2- Codifies the concept of outgoing middleware via delegating handlers in HttpClient.
3- Manages the pooling and lifetime of underlying HttpClientMessageHandler instances. Automatic management avoids common DNS (Domain Name System) problems that occur when manually managing HttpClient lifetimes.
4- Adds a configurable logging experience (via ILogger) for all requests sent through clients created by the factory.
We can use Http Client Factory in one of the following way according to requirements:
1- Basic Usage
2- Named Clients:The app requires many distinct uses of HttpClient with different configurations.
3- Typed Clients
4- Generated Clients

Basic usage
In this scenario we add a general client factory and inject it when we need HttpClient. To register it use:
Services.AddHttpClient(); (You can also add it by addSigleton for example as Services.AddSingleton(new HttpClient()));
and to Use it first inject it to some IHttpClientFactory field like _factory and then get the instance using its CreateClient method. This method altough create HttpClient instance but its MessageHandler instance managed by Factory. 

Named Http Client
Named clients are a good choice when:
    The app requires many distinct uses of HttpClient.
    Many HttpClients have different configuration.
In this scenario to register the service you should use following extension method
Services.AddHttpClient("name",config);
For example consider following:
Services.AddHttpClient("Github", options => { options.BaseAddress = new Uri("http://github.com/");});
And to Get the HttpClient you inject HttpClientFactory and then creat a named client using following command:
_factory.CreateClient("Github");

Typed Client
In this scenario we first define a class (Type) which gets an instance of HttpClient from constructor (Inject automaticly by HttpClientFactory) and then define methods for this class. Then we use this type T as generic type when registering services.
Services.AddHttpClient<T>();
And to call it in other places We inject the type T and use its methods. 
So Typed Client has following features:
* same capability like named client (we add configuration to HttpClient in T definition and also we can add Delegating Handlers to the requests )
* since we inject type T instead of HttpClient we have access to our methods by intellisense and also we have all logics related to making request in one place. 
* It can be injected directly where we need Http Client instances. 
For example we define T as below:
public class GitHubServices {
    private readonly HttpClient _client
    public GitHubServices(HttpClient client) {
        _client=client;
        _client.BaseAddress=new Uri("http://github.com/);
    }
    public IEnumerable<Object> getGitHubRepo() {
        var request=new HttpRequest(...);
        _client.sendAsync(request);
        ...
    }
}
then we register it as:
Services.AddHttpClient<GitHubServices>();
and to use it we inject it every where we required it. 
It internally works as following. The typed client is registered as transient with DI (So it is fresh and dns-updated). In the preceding code, AddHttpClient registers GitHubService as a transient service. This registration uses a factory method to:
    Create an instance of HttpClient.
    Create an instance of GitHubService, passing in the instance of HttpClient to its constructor.
As already said, The typed client can be injected and consumed directly. Also The configuration for a typed client can also be specified during its registration in Program.cs, rather than in the typed client's constructor.

Generated Clients
This is not a special case of HttpClientFactory or HttpClient. This is very another 3rd party library provide us a method to config Http Client and they internaly use a mechanism to managed it using factory for example. Consider for example Refit Library which is used for making requests to rest APIs and convert them to live Interfaces. To know more read following section of Microsoft documentation. 
https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0#generated-clients

Make Requests using HttpClient
HttpClient support Get, Post, Delet, Put and Patch methods (Http Verbs)(Too see list of complete supported method You can see HttpMethod class). To make the requests using HttpClient. The work is almost same for all method. You can use SendAsync method or its extended method like GetAsync,PostAsync,DeleteAsync,PutAsync as below:
0- To make the request in next step, you may need to make Content of request (FormUrlEncoded,string,stream,byte-array,multi-part content). JsonContent has a Create static method which used to create content.
1- Make a request using HttpRequestMessage (it has method, uri,headers and content)
2- send request using SendAsync and the returned value is response in form of HttpResponseMessage (it has status code,header,content)
3- Get Content to a desired type (string,stream,json,byte array) and use it.
The extended methods works as below:
* GetAsync method: This has not content(body) in its request and uri are necessary and Headers are optional:
var httpResponseMessage = await httpClient.GetAsync("repos/dotnet/AspNetCore.Docs/branches");
* PostAsync: it requires body. so first make body and then use it:
var todos=new StringContent(content,encoding,mediatype); 
New StringContent(JsonSerializaer.Serialize(todoItems),Encoding.UTF8,Application.Json);
var response= await _httpClient.PostAsync("/api/TodoItems", todoItemJson);
* PutAsync: This requires content: 
var response= await _httpClient.PutAsync("/api/TodoItems/1", todoItemJson);
* DeleteAsync: this does not require body and it is simple as:
var response= await _httpClient.DeleteAsync($"/api/TodoItems/{itemId}");


HttpClient
HttpClient Provides a class for sending HTTP requests and receiving HTTP responses from a resource identified by a URI. It derived from HttpMessageInvoker which has SendAsync method and it is constructed from HttpMessageHandler which is responsible for implementing SendAsync concurrently and thread safe. Most applications that are connecting to a web site will use one of the SendAsync methods on the HttpClient class.
HttpClient can be Created by Factory or by its constructor:
HttpClient client = new HttpClient(); // You can create and configure an http message handler and use 
                                    // HttpClient(HttpMessageHandler) constructor instead
HttpClient client = _factory.CreateClient();
The HttpClient class instance acts as a session to send HTTP requests. An HttpClient instance is a collection of settings applied to all requests executed by that instance. In addition, every HttpClient instance uses its own connection pool, isolating its requests from requests executed by other HttpClient instances (If it created directly using its constructor, Itself is responsible for managing connection pool). HttpClient is intended to be instantiated once and re-used throughout the life of an application. Instantiating an HttpClient class for every request will exhaust the number of sockets available under heavy loads (Here the HttpClientFactory appears!/ Another alternative is using an static field for HttpClient required in every class). The HttpClient is a high-level API that wraps the lower-level functionality available on each platform where it runs. In most of platform it uses HttpWebRequest which is a http implementation of WebRequest (Thesese are low level codes)
If an app using HttpClient and related classes in the System.Net.Http namespace intends to download large amounts of data (50 megabytes or more), then the app should stream those downloads and not use the default buffering. If the default buffering is used the client memory usage will get very large, potentially resulting in substantially reduced performance. It has following thread-safe method based on SendAsync:
0- SendAsync
1- GetAsync, GetStringAsync,GetByteArrayAsync,GetStreamAsync
2- PostAsync, PatchAsync,DeleteAsync, PutAsync
3- CancelPendingRequests

It has following properties:
1- BaseAddress(Uri): this should end with a slash (/), ow adding a relative url remove last segment.
2- DefaultRequestHeaders (HttpRequestHeaders): Gets the headers which should be sent with each request.
3- Timeout (Timespan): Gets or sets the timespan to wait before the request times out.

It has following useful extension methods (We use JsonContent which derived from HttpContent for content of request/response):
1- GetFromJsonAsync: In this method we specified serialization option and type of returned content and then Sends a GET request to the specified Uri and returns the value that results from deserializing the response body as JSON in an asynchronous operation.
2- PostAsJsonAsync: Sends a POST request to the specified Uri containing the value serialized as JSON in the request body.
3- PutAsJsonAsync:Send a PUT request to the specified Uri containing the value serialized as JSON in the request body


HttpRequestMessage
This is constructor for making Http Requests. It is disposable and can be created as:
new HttpRequestMessage();
new HttpRequestMessage(method,Uri) where uri can be a string or a Uri. 
It has following properties:
* Method (HttpMethod enumeration (It is class with static fields!) and the default value is HttpMethod.Get)
* RequestUri (Uri)
* Headers (HttpRequestHeaders)
* Options (HttpRequestOptions): Represents a collection of options for an HTTP request. It is a Dictionary<string,object> or an IEnumerable of KeyValuePair<string,object>.
* Content (HttpContent): We have described it earlier
For example we have following HttpRequestMessage (Microsoft.Net.Http.Headers.HeaderNames):
new HttpRequestMessage(
            HttpMethod.Get,
            "https://api.github.com/repos/dotnet/AspNetCore.Docs/branches")
        {
            Headers =
            {
                { HeaderNames.Accept, "application/vnd.github.v3+json" },
                { HeaderNames.UserAgent, "HttpRequestsSample" }
            }
        };

HttpResponseMessage:
HttpResponseHeaders Represents a HTTP response message including the status code and data. This is also is disposable like HttpRequestMessage. A common way to get an HttpResponseMessage is from one of the HttpClient.SendAsync(HttpRequestMessage) methods (Altough it has constructor too!). Its constructors are:
HttpResponseMessage(),HttpResponseMessage(HttpStatusCode)
Its properties are:
* StatusCode (HttpStatusCode which is an enum)
* IsSuccessStatusCode (bool)
* ReasonPhrase (string): the reason phrase which typically is sent by servers together with the status code.
* RequestMessage (HttpRequestMessage): It is not exactly what user used for example in case of Redirect we can get redirected Url from it. 
* Headers (HttpResponseHeaders)
* Content (HttpContent)

It has a useful method:
* EnsureSuccessStatusCode() which throws an error in case of non success response. 

HttpContent
Just for reviewing, it is a disposable base type which can be inherited to as JSON,String,Stream, ByteArray, Multipart,FormUrlEncodedContent or memory(?) contents. It has also Header for itself which is content-related headers like content-type (ContentHeaders). Other than Headers Property, it has lots of useful methods for extracting its content:
1- Read to different type of content
    * ReadAsByteArrayAsync
    * ReadAsStreamAsync
    * ReadAsStringAsync
2- Stream Helpers method to copy it to an stream
    * CopyTo, CopyToAsync
    * CreateContentReadStream() (returns a memory stream available in .Net 7)
    * SerializeToStream (available in .Net 7)

And then we can use other method to serialize stream for example JsonSerializer.DeserializeAsync or ReadFromJsonAsync method which do both of convert to stream and deserializing it in one method. 


HttpHeaders
This is a class which derived by HttpRequestHeaders, HttpResponseHeaders and HttpContentHeaders. It implements IEnumerable<KeyValuePair<String,IEnumerable<String>>> or simply IEnumerable and Has useful methods from IEnumerable like Add, Remove, Clear, Contains and also GetValues for itself. It of course has extension methods from Linq for IEnumerables.

HttpRequestHeaders
This derived from HttpHeaders and it has lots of method including
* Accept (HttpHeaderValueCollection<System.Net.Http.Headers.MediaTypeWithQualityHeaderValue>), 
* AcceptCharset,AcceptEncoding, AcceptLanguage (HttpHeaderValueCollection<System.Net.Http.Headers.StringWithQualityHeaderValue>):for example UTF-8;q=0.5 
* Authorization (AuthenticationHeaderValue): for example Basic YWxhZGRpbjpvcGVuc2VzYW1l
* CacheControl (CacheControlHeaderValue): This include lots of fields which separating using ; for example max-age:36000; public
* UserAgent
and lots of other useful.
Note: ContentType is a header of HttpContent and it is not the HttpRequestHeader.

HttpResponseHeaders
It derived from HttpHeaders and Represents the collection of Response Headers as defined in RFC 2616. It has lots of properties including Age, CacheControl. 


HttpContentHeaders
Represents the collection of Content Headers as defined in RFC 2616.
* Allow(ICollection<string>): List of allowed headers for HttpContent
* ContentType (MediaTypeHeaderValue): for example text/json. This is sometimes filled automaticly using selecting HttpContent we send. For example if we send StringContent, it filled with "text/plain" if we do not set it specifically in constructor. 
* ContentEncoding (string),ContentLanguage (string), ContentLength (long), ContentLocation(Uri)
* ContentDisposition(ContentDispositionHeaderValue): It is used when content include files for example in multipart HttpContent.
* Expires, LastModified both in DateTimeOffset

Other Usefull enumeration in making API calls:
* HeaderNames: this enumeration exist in Microsoft.Net.Http.Headers namespace and show all headers available.
* Application: This enumeration exist in System.Net.Mime.MediaTypeNames and show all Mimtypes like JSON.
* HttpStatusCode: this enum exist in System.Net and contain all status codes. 
* System.Net.Http.headers: this namespace has different constructor for headers. For example you can add MediaTypeHeaderValue from this namespace.
* HttpMethod: this class has constants which returns each method. You can also use its constructor to generate each method.
* Uri: this is used for converting string to url.

Outgoing Request Middleware
HttpClient has the concept of delegating handlers that can be linked together for outgoing HTTP requests. We can use HttpClientFactory to handle the outgoing request middleware using chained delegating handlers:
    Simplifies defining the handlers to apply for each named client (What about typed client? Yes them too).
    Supports registration and chaining of multiple handlers to build an outgoing request middleware pipeline. Each of these handlers is able to perform work before and after the outgoing request. This pattern:
        Is similar to the inbound middleware pipeline in ASP.NET Core.
        Provides a mechanism to manage cross-cutting concerns around HTTP requests, such as:
            caching
            error handling
            serialization
            logging
To create a Delegating Handler you should first inherit from it and then override its SendAsync Method. For example:
public ValidateHeadersHandler:DelegatingHandler {
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // do what you want before sending request to next handler (for example logging and adding api-key)
        return await base.SendAsync(request, cancellationToken);
        // do what you want after getting response (for example Serialization)
    }
}

Then you can add it using AddHttpMessageHandler extension method (Consider that they need to registered in service container first). More than one handler can be added to the configuration for an HttpClient with Microsoft.Extensions.DependencyInjection.HttpClientBuilderExtensions.AddHttpMessageHandler:
builder.Services.AddTransient<ValidateHeaderHandler>(); // Register them first
builder.Services.AddHttpClient("HttpMessageHandler")
    .AddHttpMessageHandler<ValidateHeaderHandler>(); // Add them as chain
Multiple handlers can be registered in the order that they should execute. Each handler wraps the next handler until finally HttpClientHandler executes the request:

Note: Use DI in outgoing middlewares (delegating handlers) require some consideration. Since every delegating handler creates its scope, using Scoped Services like HttpContext is not safe. HttpContext is scoped service and it has scope value according to each request but delegating handlers have scope value that change every 5 seconds. For example if you refresh page the scoped service value does not change until 5 seconds altough request value (and HttpContext scope has changed!). So to prevent this behaviour, do not rely on scope of it and scoped services. instead try to use Non scoped services like IHttpContextAccessor to get Http Context. Another alternative is to use HttpRequestMessage Properties/Options Field to pass required value to delegating handler. Another Alternative is using AsyncLocal<T> class which persist a data with type T over an async flow. Conversely we have ThreadLocal<T> which is persisted data over a thread. To read more about AsyncLocal read following:
https://docs.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1?view=net-6.0

Polly-Based Handler
When calling an API, Transient Fault may happen (Fault that resolve after a while). To have a resilient app that can work after fault, there is a 3rd party library named Polly. Polly is a comprehensive resilience and transient fault-handling library for .NET. It allows developers to express policies such as Retry, Circuit Breaker, Timeout, Bulkhead Isolation, and Fallback in a fluent and thread-safe manner. To know about it and polly-based handlers, you can see the following article:
https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0#use-polly-based-handlers

Http Client Life time management
A new HttpClient instance is returned each time CreateClient is called on the IHttpClientFactory. An HttpMessageHandler is created per named client. The factory manages the lifetimes of the HttpMessageHandler instances. HttpMessageHandler is what do handling message and can be different types: 
    DelegatingHandler - A class used to plug a handler into a handler chain.
    HttpMessageHandler - A simple class to derive from that supports the most common requirements for most applications.
    HttpClientHandler - A class that operates at the bottom of the handler chain that actually handles the HTTP transport operations (Specific to Http Client).
    WebRequestHandler - A specialty class that operates at the bottom of the handler chain class that handles HTTP transport operations with options that are specific to the System.Net.HttpWebRequest object.

If developers derive classes from HttpMessageHandler and override the SendAsync method, they must make sure that SendAsync can get called concurrently by different threads.
IHttpClientFactory pools the HttpMessageHandler instances created by the factory to reduce resource consumption. An HttpMessageHandler instance may be reused from the pool when creating a new HttpClient instance if its lifetime hasn't expired.
Pooling of handlers is desirable as each handler typically manages its own underlying HTTP connections. Creating more handlers than necessary can result in connection delays. Some handlers also keep connections open indefinitely, which can prevent the handler from reacting to DNS (Domain Name System) changes.
The default handler lifetime is 2 minutes. The default value can be overridden as below (for named client):
Services.AddHttpClient("github").SetHandlerLifetime(TimeSpan.FromMinutes(5));
Altough HttpClient is disposable but we should not dispose them manually since IHttpClientFactory tracks and disposes resources used by HttpClient instances. Keeping a single HttpClient instance alive for a long duration is a common pattern used before the inception of IHttpClientFactory.
Using IHttpClientFactory in a DI-enabled app avoids:
    Resource exhaustion problems by pooling HttpMessageHandler instances.
    Stale DNS problems by cycling HttpMessageHandler instances at regular intervals.
The alternative for this is using SocketsHttpHandler as http message handler:
    1-Create an instance of SocketsHttpHandler when the app starts and use it for the life of the app.
    2-Configure PooledConnectionLifetime to an appropriate value based on DNS refresh times.
    3-Create HttpClient instances using new HttpClient(handler, disposeHandler: false) as needed.
Considering above instruction, the following are true here:
    The SocketsHttpHandler shares connections across HttpClient instances created in step 3. This sharing prevents socket exhaustion.
    The SocketsHttpHandler cycles connections according to PooledConnectionLifetime to avoid stale DNS problems.
You can not use this approach on Blazor Web Assembly since 'SocketsHttpHandler' is unsupported on: 'browser'.

Logging in Every API calls
when an api call happens, first a chain of delegating handler do some action on requests and then a http message handler (HttpClientHandler/ SocketsHttpHandler) do actual request and return response and then response revers delegating handlers chain to the app. 
Clients created via IHttpClientFactory record log messages for all requests. Enable the appropriate information level in the logging configuration to see the default log messages. Additional logging, such as the logging of request headers, is only included at trace level.
The log category used for each client includes the name of the client (For example Default for unnamed http client). And according to handlers we have two category here:
* System.Net.Http.HttpClient.Default.LogicalHandler: Messages suffixed with LogicalHandler occur outside (Just Before or just after) the request handler pipeline. On the request, messages are logged before any other handlers in the pipeline have processed it. On the response, messages are logged after any other pipeline handlers have received the response
* System.Net.Http.HttpClient.Default.ClientHandler: For the request, this occurs after all other handlers have run and immediately before the request is sent. On the response, this logging includes the state of the response before it passes back through the handler pipeline.

Configure the http message handler used for HttpClient
When adding HttpClientFactory to services collection, it return an oject of IHttpClientBuilder which has extended methods for configuring the httpmessagehandler/httpclient/httpclientfactory. This interface has following methods:
* SetHandlerLifetime: used for setting default lifetime of each handler used
* ConfigurePrimaryHttpMessageHandler: used for configuring http message handler. This should return a handler
* AddHttpMessageHandler:used to create an additional message handler
* ConfigureHttpClient: used for configuring http client
* AddTypedClient: with this method we can bind a named http client with Typed Http client
* RedactLoggedHeaders: this is used to hide a collection of headers values in logging

For example you can configure http message handler as below (As you can see we configure client itself inside AddHttpClient and its handler in ConfigurePrimaryHttpMessageHandler):
builder.Services.AddHttpClient("named", 
    (options) => { options.BaseAddress = new Uri("https://google.com/"); })
    .ConfigurePrimaryHttpMessageHandler(() =>
       {
           return new HttpClientHandler()
           {
               UseDefaultCredentials = true,
           };
       });

We can also chain configuration of HttpClient as below:
builder.Services.AddHttpClient("nsm").ConfigureHttpClient(options =>options.BaseAddress=new Uri(""));

Use Cookie in HttpClients
Http Client can send cookies from app (client) to server when calling an API (consider that this happens in .Net Host) and pooling http client handler result in shared CookieContainer which may result in unanticipated results. so disable automatic cookie handling in configuring http message handler or do not use http client factory when sending cookies are required. To disable automatic cookies handling in http message handler do following:
Services.AddHttpClient("NotAutomaticCookie").ConfigurePrimaryHttpMessageHandler(()=>new HttpClientHandler() {UseCookies=false})

Using Http Clients in Console App
since Http Client depends on some package, we should add them when using Http Client in Console App:
1- It needs Host so we should create a host which requires Microsoft.Extensions.Hosting
2- To add IHttpClientFactory we should add Microsoft.Extensions.Http package
3- We need Dependency Injection package Microsoft.Extensions.DependencyInjection paackage to add services and DI pattern
to use them we should first create a generic host and add http client to its services container. Then Build the host and use its service container to get Required services (Which is Http Client or TypedClient here).

var host=new HostBuilder().ConfigureServices( services =>{
    
    services.AddHttpClient<GitHubServices>();
}).Build();
try {
    var client=host.Services.GetRequiredServices<GitHubServices>();
    var docs=await client.getDocs();
}  catch(Exception ex) {
    var logger=host.Services.getRequiredServices<ILogger<Program>>();
    logger.LogInformation(ex.Message);
}

Http Headers Propagation middleware
You may want to use headers in current request in Callin an api using Http Client. To do this you should use a middleware as below (requires Microsoft.AspNetCore.HeaderPropagation package):
Services.AddHttpClient("name").AddHeaderPropagation();
Services.AddHeaderPropagation(options =>{
    options.Headers.Add("api-key");
});
...
app.UseHeaderPropagation();

Call APIs in Blazor
Calling API is not different from rest of ASP.Net, but it requires some consideration
* Blazor Server runs in server so we do not require to do anything for CORS (Cross orging resource sharing) but in case of web assembly when it loads in browser, we should consider CORS in our endpoint development (Something in our api and not here!! Of course in case of hosted we can add a proxy in our server for this purpose). 
* in Blazor Server we add HttpClient Factory to manage http client as we do in Hosted server for Blazor Web Assembly. But in case of Blazor Web Assembly hosted in browser, we can use singleton HttpClient (It is not necessary in case of current host, since already it is added in host according to next note). In Blazor server if we inject HttpClient it insert the default http client to component but it is better to inject Http client factory first and then create client.
* Blazor WebAssembly apps call web APIs using a preconfigured HttpClient service, which is focused on making requests back to the server of origin. Additional HttpClient service configurations for other web APIs can be created in developer code. Requests are composed using Blazor JSON helpers or with HttpRequestMessage. Requests can include Fetch API option configuration.
* Named and Typed Http Client supported in Web Assembly but it required following namespace to enable adding them: Microsoft.Extensions.Http


Note: To a list of objects to input fields and add item to list, define an add button which add simply a new object using constructor. Since every item in Array/List are bound to inputs, changing it focus on item and array. When using foreach (var item in List) item are referenced to the items and not created again. So changing is 2-way (bound).

Note: JsonContent and System.Net.Http.Json.HttpClientJsonExtensions
When the content of http request/response is json we can use JsonSerializer from System.Text.Json or directly use extension method on Content to serialize/deserialize json contents. Also we can create json content using JsonContent.Create static method. The create method has following overloads:
Create(Object, Type, MediaTypeHeaderValue, JsonSerializerOptions)
Create<T>(T, MediaTypeHeaderValue, JsonSerializerOptions)
It has 3 fields Headers, ObjectType and Value. Value is what serialized and used to generate body of HttpResponseMessage (Automaticly!). The HttpContent/JsonContent has an extented method: ReadFromJsonAsync which reads the content and deserialized it as object when we get a response (used on response.content). We can alternatively use methods to send an object directly as JsonContent as below (On HttpClient instance):
1- PutAsJsonAsync: this use an Object T as body of request
2- PostAsJsonAsync: this use an Object T as body of request
or get and deserialize response using (again on HttpClient instance):
GetFromJsonAsync: this is same as getting a request and then use ReadFromJsonAsync.

How HttpClient Work in Blazor Web Assembly Client side:
It is using Fetch API from java script and that is subject to its limitations, including enforcement of the same-origin policy (CORS)
The client's base address is set to the originating server's address for default http client. Inject an HttpClient instance into a component using the @inject directive (the default instance which has a set base address!)
As already said HttpClient implemented using Fetch API in js. Fetch has multiple options other than Request/Resource. It has following configuration which You can use related .Net method to apply them.
1- Credentials: It can be omit, same-origin, include. This controls data related to credential which may be (Cookies, Http Authentication Entries (Authorization Header in Request and WWW-Authenticate Header in response. There exist Proxy-Authorization and Proxy-Authenticate similarly. We also have Set-Cookie Header in response), TLS certificate). This handle in .Net with a extension method for HttpResponseMessage named SetBrowserRequestCredentials:
request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
2- Cache: .Net handle this using request.SetBrowserRequestCache(BrowserRequestCache.OnlyIfCached). It has following options:
https://developer.mozilla.org/en-US/docs/Web/API/Request/cache
3- RequestMode: It can be cors,no-cors or same-origin. It can be set in .Net using:
request.SetBrowserRequestMode(BrowserRequestMode.Cors);
https://developer.mozilla.org/en-US/docs/Web/API/Request/mode
4- request.SetBrowserRequestIntegrity("xxx") vs request integrity
5- request.SetBrowserResponseStreamingEnabled(true); vs Response as ReadableStream. If this is false fetch buffer the response and then fullfilled. but in this case you have an stream which can be used using ReadableStream APIs.
6- request.SetBrowserRequestOption("redirect", "follow"); for setting other configs in Fetch

Note for JS Fetch API: The body type can only be a Blob, BufferSource, FormData, URLSearchParams, USVString or ReadableStream type, so for adding a JSON object to the payload you need to stringify that object. This is true for HttpContent in .Net. If you use JsonContent you should use PostAsJsonAsync which automaticly do this. In other HttpContent it is done already. 
Note For JS Fetch API: When using Request or Response constructor/ objects you have following methods on it which is equal to methods in .Net:
1- response.blob() (request.blob()):  reads the request body and returns it as a promise that resolves with a Blob
2- response.arrayBuffer() (request.arrayBuffer()):  reads the request body and returns it as a promise that resolves with an ArrayBuffer
3- response.json() (request.json()): reads the request body and returns it as a promise that resolves with the result of parsing the body text as JSON. Note that despite the method being named json(), the result is not JSON but is instead the result of taking JSON as input and parsing it to produce a JavaScript object. This object could be anything that can be represented by JSON — an object, an array, a string, a number
4- response.formData (request.formData()): reads the request body and returns it as a promise that resolves with a FormData object. formData is equal to MultipartFormDataContent in .Net.
5- response.text() (request.text()):  reads the request body and returns it as a promise that resolves with a String. The response is always decoded using UTF-8. 
6- response.body (request.body) property: The read-only body property of the Request/Response interface contains a ReadableStream with the body contents that have been added to the request. Consider that even in case of string body this property return readable stream. 

There is some useful properties for response like response.ok, response.status, response.redirect (bool), .... and resquest has properties like request.mode, request.credential,request.cache, request.method, ... which described already. 
The othere important constructor in fetch is Headers. both response and request has a property headers with this type of info (cookie/set-cookie is considered header). Header has like a collection and has for ... of structure (iterables). it has following methods:
1- append 2- set 3- delete 4- get 5- keys 6- entries 7-values 8- has
It has other constructue which directly set headers as object or Array of Arrays:
var myHeaders = new Headers({
    'Content-Type': 'text/xml'
});

// or, using an array of arrays:
myHeaders = new Headers([
    ['Content-Type', 'text/xml']
]);
In .net we separate HttpContent Headers and request Headers but in fetch both are composed with each other. 








