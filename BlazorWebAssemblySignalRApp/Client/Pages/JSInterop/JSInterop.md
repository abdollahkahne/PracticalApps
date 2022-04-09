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

Note: Altough all object convert using json serialization to an input of js function, in case when An ElementReference is passed through to JS code via JS interop. The JS code receives an HTMLElement instance, which it can use with normal DOM APIs. Only work with DOM directly for an element which has not any parameter or content which managed by Blazor (except the @ref). Other wise you may see unwanted behaviour.

Note: One of use case of JS Interop is to use browser APIs from .NET. there is lots of APIs developed for browser which may be useful in some scenarios. To see the list of browser API you can see:
https://developer.mozilla.org/en-US/docs/Web/API

Note: Some browser API, due to security consideration, can only be triggered by user and can not triggered in code (API restericted to user gesture). For example consider the case the api for making a video playback in full screen. To do them we should trigger them as an event raised by user (For example onclick). But in case of Blazor Server since it send to server through another event which is not raised by user (signalR), the method do not execute if we use @onclick which handle by Blazor. In this case you should handle the method in js and using onclick trigger totally in JavaScript context. 

Note: For Blazor Server apps with prerendering enabled, calling into JS isn't possible during initial prerendering. JS interop calls must be deferred until after the connection with the browser is established. We get the following error in case of using JS Interop on Serverside:
JavaScript interop calls cannot be issued during server-side prerendering, because the page has not yet loaded in the browser. Prerendered components must wrap any JavaScript interop calls in conditional logic to ensure those interop calls are not attempted during prerendering.
Basically do not do any js/dom manipulation untill afterRender called. 

Note:
 Directly modifying the DOM with JavaScript isn't recommended in most scenarios because JavaScript can interfere with Blazor's change tracking

Use JS Interop in class
We used JS Interoperability in Razor component above. but if we want to use them in all c sharp class we can do same. Inject IJSRuntime and use it. So we should use Microsoft.JSInterop namespace here. 
Then we can use it by instantiation in OnInit life step. We can not use Inject Service if we do not register them in Service collection, but we can register it if we want to use it on multiple places.
What happen if we call a method that need browser to execute (For example do UI change) in Blazor Server?! No I do not think it works!
JavaScript interop calls cannot be issued during server-side prerendering, because the page has not yet loaded in the browser. Prerendered components must wrap any JavaScript interop calls in conditional logic to ensure those interop calls are not attempted during prerendering.

Use JS Interop in Razor Component in CSharp classes using BuildRenderTree method
in those cases we cane inject it using [inject] attributes as below:
[Inject]
IJSRuntime JS { get; set; }

Note:
To delay JavaScript interop calls until a point where such calls are guaranteed to work, override the OnAfterRender{Async} lifecycle event and not in any earlier lifecycle method because there's no JavaScript element until after the component is rendered (At first render!). This event is only called after the app is fully rendered (and not run in Prerendering!).

Impoprt JS Module from Razor Component or CSharp Class
import the module into the .NET code by calling InvokeAsync on the IJSRuntime instance. IJSRuntime imports the module as an IJSObjectReference, which represents a reference to a JS object from .NET code. Use the IJSObjectReference to invoke exported JS functions from the module.
Dynamically importing a module requires a network request, so it can only be achieved asynchronously by calling InvokeAsync. IJSInProcessObjectReference represents a reference to a JS object whose functions can be invoked synchronously. IJSInProcessObjectReference types has synchronos Invoke<T> and InvokeVoid too.Similary we have IJSInProcessRuntime which inherit from IJSRuntime.

Note: Disposes the IJSObjectReference for garbage collection in IAsyncDisposable.DisposeAsync.
Note: Objects that contain circular references can't be serialized so they can not pass as arguments or return types both in js invocation in dotnet and dotnet invocation in JS.

Extending ElementReference API
We know that java script has lots of APIs for html elements that can be chained together. Could we have those APIs or custom APIs on it?
Yes we can use js interop for that purpose and then define an extension method which calls that js function. The only thing is that in the static method we should provide IJSRuntime as argument to the extension method.
Consider that in this cases You can not raise an event and handle it both by Blazor (See example JSInteropComponent). As you can see in the example we raise the click event by extending the Element Reference and then call it in an event call back (For example a button click) but to handle it we have to use onclick instead of @onclick. since the first handle by JavaScript and second handle by Blazor and as you know blazor is single thread and it can do one of both event handler and event raiser method simulatenously. Altough as you see it is a .Net Delegate too but it called without any side effect on blazor like Rerendering!

Passing Element Reference to Child Component
We can not pass an ElementReference to children through Parameters since:
1- Parameters can not be struct (It is not reference type or primitive)
2- ElementReference is not available till OnAfterRender.

But we can register a callback from Child and Invoke it in OnAfterRender when ElementReference is available.
For this purpose we can use one of the following two patterns (an a method just for small cases):
0- Use a reference to child component and then call appropriate method from parent in OnAfterRender
1- Publisher and Subscribers: Create an event in the parent component which raised in OnAfterRender with data we want to share and we add event handleres in Child Component. The edit form have same pattern in editcontext for custom validation for example. We implemented that in JSInteropComponent too. To do that you can do following:
    * Create a class which has an event and event raise
    * insantiate it in parent and send it as parameter to children
    * in child subscribe to event of the instance and in the parent raise the event on OnAfterRender
2- Observable and Observers: To implement it you can do following (Use Partial class for component for readability)
    * Create parent as an Observable Component: this component should implement IObservable<ElementReference> and be Disposable: Have a list of observers and triggers OnNext and OnComplete event for observers and a method which manage subscribe to it. 
    * Creat children as Observer components. they should implement IObserver<ElementReference> and be Disposable and Has 3 main method OnNext,OnComplete,OnError and Have specified the Observable and also subscription (Disposable returned from Subscribe method) as a property.
    * The component better to implements partialy

JS Interop in Blazor Server
In Blazor Server apps, JavaScript (JS) interop may fail due to networking errors and should be treated as unreliable.
In blazor JS call do in browser side (Not in serverside!) but it send a signalR message to server in every step of execution to the server and return the result from server. If JS do not return any thing it does only send a signal that specify successful execution of JS. 
By default, Blazor Server apps use a one minute timeout for JS interop calls. If an app can tolerate a more aggressive timeout, set the timeout using one of the following approaches:
1- set default timeout in service configuration in timespan (services.AddServerSideBlazor(options => options.JSInteropDefaultCallTimeout = {TIMEOUT}))
2- Set Timeout in every invocation (Timespan). for example: JS.InvokeAsync<T>("import",{TimeOut},arguments); this is usefull in Blazor Web Assembly too. 

Another concern in Blazor Server is size limit on every message. The default size limit from client to Hub is 32K which may not be enough for some cases. Yo can add it in hub options as HubOptions.MaximumReceiveMessageSize=32768. 
Services.AddServerSideBlazor()
    .AddHubOptions(options => options.MaximumReceiveMessageSize = 64 * 1024);
 JS to .NET SignalR messages larger than MaximumReceiveMessageSize throw an error. The framework doesn't impose a limit on the size of a SignalR message from the hub to a client.
 Increasing the SignalR incoming message size limit comes at the cost of requiring more server resources, and it exposes the server to increased risks from a malicious user. Additionally, reading a large amount of content in to memory as strings or byte arrays can also result in allocations that work poorly with the garbage collector, resulting in additional performance penalties. There is some measures which can be taken here. for example:
 * Leverage Native streaming Interop between js and dotnet (later section)
 * Don't allocate large objects in JS and C# code.
 * Free consumed memory when the process is completed or cancelled
 * For security purposes 1- Declare the maximum file or data size that can be passed. 2- Declare the minimum upload rate from the client to the server. (How??)
 * In case of storing data in server: 1- Temporarily stored in a memory buffer until all of the segments are collected. 2- Consumed immediately (store in database or write on disk)

Use JavaScript libraries that render UI (For example JQuery UIs)
Altough this is not recommended in diff-based UI frameworks but fortunately Blazor give a chance for that in razor component files (.razor) [this is not true in class based??] for empty element. For example you can add as many empty div element inside a Razor component and use @ref to link it with external library.  As far as Blazor's diffing system is concerned, the element is always empty, so the renderer does not recurse into the element and instead leaves its contents alone. This makes it safe to populate the element with arbitrary externally-managed content.I think, Same is true for component with only a static text node since for Blazor diffing system dom before and after are same.
The best place to interact with external library is inside OnAfterRender in first rendering (Client side always and only one time). Blazor leaves the element's contents alone until this component is removed. When the component is removed, the component's entire DOM subtree is also removed.
Retaining an element
DOM elements are retained where possible by default. We can also use @key directive to preserve elements to prevent undesirable behaviours in case of List elements. You can also use @key to preserve an element or component subtree when an object doesn't change (non list use). So another approach for working with UI libraries and framework is using a value that does not change. There's a performance cost when rendering with @key. The performance cost isn't large, but only specify @key if preserving the element or component benefits the app and the element is not empty. Just for reminding, Even if @key isn't used, Blazor preserves child element and component instances as much as possible.

Byte Array To String in Blazor
Basically a byte array converted to Base64 string and then send over the lines. but in Blazor since argument serialized using JSON Serializer and in json bytes serialized to UTF-8 all we need is to use a simple TextDecoder and then decode the received parameter as below:
function decodeByteArray(bytes) {
    var decoder=new TextDecoder();
    var str=decoder.decode(bytes);
    return str;
}
 where bytes defined as byte[] {} (Byte Array) in dotnet and then use the above js function through JS Interop. 

 just for learning here we describe base64 and convertion of binary data to it here. 
 consider we have an array of byte. if we split it every 3 bytes we have 3*8=24 bits which is equal to 6bit*4 which we name it base64.
 we can save from 0 to 63 in 6 bit which are [A-Z][a-z][0-9]+/
 add to the above ascii code = as padding (end of payload) ( when the length of the unencoded input is not a multiple of three, the encoded output must have padding added so that its length is a multiple of four. The padding character is =)
 So here we have another relation to 64.
 There is to function in js which convert between binary and base64 and reverse as below:
 btoa() which is not Base64 to something but it is Binary to Ascii (Base64) or in other way it is beautifull to awful!! it convert any thing to base64.
  for example to convert a utf8 text to base64, btoa use following:
 0- consider utf8 string str="This is a sample"
 1- it get binary representation of string for example we can get it as below:
 var encoder=new TextEncoder(); // same is new TextEncoder("utf-8"). We also have TextDecoder.
 encoder.encode(str); // It get an array buffer (Typed Array) Uint8Array which is an array of bytes in js.
 2- Get base64 string encoding of it as described above (3*8=6*4)

atob() which is not to base64 again! but it is ascii to binary or awfull to beatiful and do the reverse of above and convert base64 string to binary representation of it. 

let text = "This is a sample";
let encoded = window.btoa(text); // only accept text which is representable by one byte or use BufferArray
let decoded = window.atob(encoded);

The following Articles describe BufferArray (which can constructed from local files or base64 string) and different Typed Array exist in JS which they does not differ except the allowed value in array:
https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/ArrayBuffer
https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/TypedArray
https://developer.mozilla.org/en-US/docs/Web/API/TextDecoder
https://developer.mozilla.org/en-US/docs/Web/API/TextEncoder

What is Marshalling and Unmarshalling
Marshalling is the process of transforming the memory representation of an object into a data format suitable for storage or transmission. It is typically used when data must be moved between different parts of a computer program or from one program to another. 
Marshalling can be somewhat similar or synonymous to serialization. Marshalling is describing an intent or process to transfer some object from a client to server, intent is to have the same object that is present in one running program, to be present in another running program, i.e. object on a client to be transferred to and present on the server. Serialization does not necessarily have this intent since it is only concerned about transforming data into a, for example, stream of bytes. One could say that marshalling might be done in some other way from serialization, but some form of serialization is usually used.
It simplifies complex communications, because it uses composite objects in order to communicate instead of primitive objects. The inverse process of marshalling is called unmarshalling (or demarshalling, similar to deserialization). An unmarshalling interface takes a serialized object and transforms it into an internal data structure, which can be referred to as executable. In some context Serialization and Marshalling used interchangeably. 

Unmarshalled JS Interop
As you know the data between js environment and Razor environment passed using json serialization or marshalling. But sometime it may be costly from performance porint to do that. For example consider:
1- Marshalling a high volum of data: for example consider that when mouse move on screen we send on every pixel change the mouse move object (clientX,clientY at least) to Razor. 
2- Marshalling Large Object for example a file

In this case we may not do serialization and use in Razor IJSUnmarshalledObjectReference type to send or receive input and output. To work with .Net  (Razor) object in JS you can use Blazor.Platform object and BINDING object which provide methods to read and write to them.
IJSUnmarshalledObjectReference Represents a reference to a JavaScript object whose functions can be invoked synchronously without JSON marshalling. It is a .Net type and used in .Net. Also since it works with memory it is sync if does not require another async operations and so it doesnt have the overhead of serializing .NET data.
To see and example see UnmarshalledJSInterop component. The instruction is as below:
1- There is two unmarshalled types: IJSUnmarshalledRuntime and IJSUnmarshalledObjectReference. This types has methods for work synchronously but here we only consider the method InvokeUnmarshalled which is a generic method with signature InvokeUnmarshalled<T1,T2,TResult>("functionname",T1 struct,T2Items) which return TResult object or struct (T2 can be removed and they can primitve types and used directly without further attempts). IJSRuntime and IJSObjectReference can easily be casted to this types. So you should inject IJSRuntime and then cast or import IJSObjectReference js module or objects and cast it and then use its method in unmarshalled way.
2- As I said in step 1, input and output types in unmarshalled way are primitive which can be readed directly in js and can be returned directly from js. But to Read other types, we should use struct with explicit layout as input and then use methods provided in Blazor.platform object to read its field from memory in unmarshalled way. For example to read an string in Offset 0 or and int in Offset 8 you can use readStringField(structInp,0) and readInt32Field(structInp,8) respectively. Then work with them in js using JavaScript functions and apis
3- To return a value from JS you can return primitive in normal way but to return other types like String,Array,Struct or Object or Enum, you should use BINDING.js_to_.. methods which create the return value in memory in a way that can be readable by .Net.

So we have 3 following JS Interop Runtime/ObjectReference which can be cast to each other:
1- IJSRuntime and IJSObjectReference:only can call asynchronously. The input and return value serialized between two runtimes (.Net and JS)
2- IJSInprocessRuntime and IJSInProcessObjectReference: can call both sync and async and input and return value serialized.
3- IJSUnmarshalledRuntime and IJSUnmarshalledObjectReference: This is used for unmarshall JS Interop which means one of the input or return type are Unmarshalled (Only its InvokeUnmarshalled works for unmarshalled). Consider that when you use InvokeUnmarshalled Serialization does not do for input and return value and so we should use BINDING for return or use IJS... types for returned value and use primitive or structs for inputs. 

Struct Layout 
Blazor.Platform has methods that read fields from memory by using Field Offset directly in js. Field Offset specify exactly where the field saved in memory allocated to a struct or class object. To specify them we use StructLayout and FieldOffset Attributes in type definition (System.Runtime.InteropServices namespace). 
First review the definitions:
* StructLayout control the physical layout of the data fields of a class or structure in memory. You can apply this attribute to classes or structures. It can be Sequential or Explicit.
* LayoutKind.Sequential: The members of the object are laid out sequentially, in the order in which they appear when exported to unmanaged memory. The members are laid out according to the packing specified in Pack, and can be noncontiguous. This is default for structs and necessary if you want to work with it in unmanaged way too (For example Datetime field set by PC time ). 
* Layout.Explicit: The precise position of each member of an object in unmanaged memory is explicitly controlled, subject to the setting of the Pack field. Each member must use the FieldOffsetAttribute to indicate the position of that field within the type.
* Layout.Auto: The runtime automatically chooses an appropriate layout for the members of an object in unmanaged memory. Objects defined with this enumeration member cannot be exposed outside of managed code. This is default for class. 
* FieldOffset Indicates the physical position of fields within the unmanaged representation of a class or structure. They required for a struct or class with an explicit layout. It specifies offset from the beginning of the structure to the beginning of the field.
* Blittable Types are data types in the Microsoft . NET Framework that have an identical presentation in memory for both managed and unmanaged code. they do not require conversion when they are passed between managed and unmanaged code.
* Managed Code vs Unmanaged Code: Managed code is the one that is executed by the CLR of the . NET framework while unmanaged or unsafe code is executed by the operating system. The managed code provides security to the code while undamaged code creates security threats.
* Managed Memory vs Unmanaged Memory: The Microsoft definition is that managed memory is cleaned up by a Garbage Collector (GC), i.e. some process that periodically determines what part of the physical memory is in use and what is not. Unmanaged memory is cleaned up by something else e.g. your program or the operating system.

The common language runtime (CLR) controls the physical layout of the data fields of a class or structure in managed memory. However, if you want to pass the type to unmanaged code, you can use the StructLayoutAttribute attribute to control the unmanaged layout of the type. Use the attribute with LayoutKind.Sequential to force the members to be laid out sequentially in the order they appear. For blittable types, LayoutKind.Sequential controls both the layout in managed memory and the layout in unmanaged memory. For non-blittable types, it controls the layout when the class or structure is marshaled to unmanaged code, but does not control the layout in managed memory. Use the attribute with LayoutKind.Explicit to control the precise position of each data member. This affects both managed and unmanaged layout, for both blittable and non-blittable types. Using LayoutKind.Explicit requires that you use the FieldOffsetAttribute attribute to indicate the position of each field within the type.

C#, Visual Basic, and C++ compilers apply the Sequential layout value to structures by default. For classes, you must apply the LayoutKind.Sequential value explicitly.

Streams
since streams can not be serialized without buffering, we can use them from .Net in JS using DotNetStreamReference. This streams create from .Net stream and has two parameter (stream and leavopen=false). You can get the stream in js both as stream and as ArrayBuffer. 
* Use BufferArray: const data = await streamRef.arrayBuffer();
* Use ReadableStream: const stream = await streamRef.stream();
Since DotnetStreamReference is disposable we should use it in using. The reverse also possible from JS Stream to .Net stream through IJSStreamReference.

Catch JavaScript Exceptinh
When you catch a js some error may happen. To catch them you can catch JSException in a try-catch. It message says the detail of it.


Call .Net methods from JS function:
We can also call .Net Method in JS function using DotNet Object Provided by Blazor js script.
* Call Static Methods
First we should add the JSInvokable Attribute to the static method. The method also should be public. Then you can call the method in js using DotNet.invokeMethodAsync or DotNet.invokeMethod. The first one is an async JS function which return promise as result which we can use async-await in js too (DotNet.invokeMethod returns the result of the operation. DotNet.invokeMethodAsync returns a JS Promise representing the result of the operation.). 

Signature of methods in js are:
DotNet.invokeMethod(AssemblyName,MethodName,argument as comma separated items); The arguments is optional and each of them should be JSON serializable. Also the return value could be serializable and be convertable to .Net types.
Note: By default, the .NET method identifier for the JS call is the .NET method name, but you can specify a different identifier using the [JSInvokable] attribute constructor.
Note: To support Blazor Server Scenarios, use async version. 
Note: Calling open generic methods (T is used in function name??) isn't supported with static .NET methods but is supported with instance methods, which are described later.
Note: To assign JS function to an event, use normal html DOM on... event not Blazor @on... events (for example use onclic vs @onclick). The Blazor one is for calling dotnet methods directly.

*Call .Net Instance method
When .Net method are not static, we should pass an instance of the class to the js function and then use the instance method from it with invokeMethod and invokeMethodAsync of the reference of instance. To undrestand it, consider js global function and js modules in js as Static method and instance method respectively. when we want to use the modules functions, we use an IJSObjectReference pointing to js module and then invoke its exported functions by invoking from the reference. Same is true here. You consider the instance as js module. We create a reference to it by creating a DotNetObjectReference for that instance and then use its instance method (which are JSInvokable) from the reference by invokeMethod and invokeMethodAsync. For static method in .Net(like global functions in js), we use DotNet.invoke... in js (IJSRuntime instance in .Net) to invoke them. 
Then change made by js apply in passed instance using JSON Serialization so we can use that instance in .Net side to get its field and properties with updated values. 
It is disposable and like IJSObjectReference can be create,use and dispose in both environments (.Net Blazor vs JS. To avoid a memory leak and allow garbage collection, the .NET object reference created by DotNetObjectReference (in .Net environment) is disposed in the Dispose method of component. Does disposing the reference disposes the Value too?  No, It does not call the disposal of Value method and if it is disposable too, you should do it yourself!
Note: You can pass method arguments to invokeMethod as second to end arguments after Method invokable name if they are JSON Serializable.

Use a .Net Object in multiple .Net method or use multiple method of a .Net Object from JS
When we want to use a dotnet Object in multiple js function, You can create a js class with Static Methods. It should has an static property too. You can create a static Create method in the class which get an instance of JSObjectReference from await IJSRunTime.invokeAsync("Create",reference) and then you can similary call other js method which uses the static object created from reference in static Create method. You can use js method in JS DOM Event or other method simply by this approach. This approach should be implemented globally if you want to use JS DOM Events.
Note: The optional arguments of .Net method should be set in JS calls.
Note: The events handled by JS DOM events do not call Re-rendering after event handling and we should call event state has changed ourselves. But to call it we should consider that the method StateHasChange is a .Net method which is not JSInvokable too. So to call it we have two options and consider that we should apply it in context of current component Synchronization context:
1- Send a reference to this component (DotNetObjectReference.Create(this)) and then call it StateHasChanged method and if it required use its InvokeAsync too.
2- Implement a notification system: Create a class which has an event and event raiser. Raise its event by using current sent DotNetObjectReference (Add a method to it!) and add an event handler in component to response to the notification and apply upates.
Note: We can also invoke JS Method in classes as you know already and similarly you can pass any class instances to js side by using DotNetObjectReference.

Question: Consider we have a JS Code which calls a .Net Method by its name and we have more than of one instance of the component which own .Net method. If we pass a dotnet object reference to this keyword (Component instance) Blazor work as expected but there is other thechnique which proposed by documentation:
1- Create a Helper Class (This helper class is nothing except a class which has a field _action which injected from constructor and it has a method which is JSInvokable and simply do _action.invoke). In Razor components instead of sending an object reference of instance of Razor Component we simply send a reference of instance of the helper class. To create the instance of helper class we use its constructor with an action related to none JSInvokable and may be private method of component instance. The difference is here that in this method we expose instance of helper class and not the instance of Razor Component. I am not sure what is the profit of using this approach over making the component method JSInvokable and then invoke it in JS but this is recommended in Documentation of microsoft. But the pattern is a good patern for working with methods using Helper class and may be useful other places too. 

A helper class can invoke a .NET instance method as an Action. Helper classes are useful in the following scenarios:
    When several components of the same type are rendered on the same page.
    In a Blazor Server apps, where multiple users concurrently use the same component.

Stream Input/Return of .Net Method when calling the method from JS
If .Net method require a stream input then JS Stream are not directly usable there since they can not get serialized. Same as calling JS function requiring stream (which we send a reference to stream using DotNetStreamReference), we can send a JS Stream (It accept only Blob and TypeArray and do not accdpt readable or writeable streams directly) to .Net using IJSStreamReference Dotnet Type. Here we should define .Net method in a way that it gets input with IJSStreamReference Type instead of Stream itself. Same is true for returning an stream from JS method invocation. If a method of .Net return stream, and it should be used in JS, we should define a middle method which converts this stream to DotNetStreamReference and then use it in JS. 
IJSStreamReference is somehow same to IBrowserFile Interface. They both have a method OpenReadStreamAsync which returns an stream for us to work in .NET with it. both of them have size limit and cancellation token optionally (512KB size limit by default)
Stream Type in JavaScript are supported by 


DotNetObjectReference Type
We have types DotNetObjectReference (static) and DotNetObjectReference<T>(non static and disposable) which is used to send an instance of a Type from .Net to JS. We also have DotNetStreamReference which we used when sending stream from .Net to JS. To create an instance of DotNetObjectReference<T> we should use its static method Create which accept an instance of T (DotNetObjectReference is static and we can not create it using new and constructor! but DotNetObjectReference<T> is not static and be instantiate using Create method. It has a property Value which has the value of Type created by Reference). for example to create an instance of DotNetObjectReference<Student> we should do:
var studentRef=DotNetObjectReference.Create(new Student());
This method has following apis on .Net and also in JS:
* .Net Methods and Properties:
1- Value Property which shows the value which its references created

* JS Apis
1- invokeMethod("methodIdentifier",{arguments});
2- invokeMethodAsync("methodIdentifier",{arguments});
3- serializeAsArg()
4- dispose()
5- constructor(obj): which get a DotNetObject for Object pass as parameter.

JS version of DotNetObjectReference is DotNetObject. 
As you alread know, To create an instance of DotNetStreamReference we used its constructor as new DotNetStreamReference(stream) and then send it to JS function and it has two main method as arrayBuffer and stream in JS which described already about them. 

DotNet Global Object 
This object is usefull for calling .Net method from JS and also provide multiple api for JS-Interop as below:
1- invokeMethod, invokeMethodAsyn: call .net method in JS
2- [DotNetObject]: This is JS Version of .Net DotNetObjectReference Objects. It created in .Net and passed to JS but it is also possible for JS to create an instance of this by itself. So from JS side, DotNetObject is a js class (constructor) for creating .Net Object in JS which can be used as a sub of DotNet itself which means can invoke methods and is disposable and has a method serializeAsArgument to use it in js function and of course can be return as return type of invoking DotNet methods). For .Net side of it see DotNetObjectReference above.
3- JSCallResultType:This is an enumeration of result of calling a JS method in .Net which can be Default, JSObjectReference, JSStreamReference, JSVoidResult or other .Net types. (Not sure where it can be used?)
4- createJSObjectReference and disposeJSObjetReference: methods for creating and disposing a JSObjectReference in JS which can be used as argument in invoking .Net method in js.
5- createJSStreamReference: for creating JSStreamReference which then can be used as stream in DotNet method arguments when invoking them. 
6- attachReviver:Possibly add reviever to json serialization???
7-attachDispatcher 
8-jsCallDispatcher: this function?Object? has usefull helper methods for JS Interop. For example you can get a ByteArray from .Net using receiveByteArray or supply an stream to a .Net Method using supplyDotNetStream


Stream in JavaScript
With Streams being available to JavaScript, you can now start processing raw data with JavaScript bit by bit as soon as it is available on the client-side, without needing to generate a buffer, string, or blob.
There are more advantages too — you can detect when streams start or end, chain streams together, handle errors and cancel streams as required, and react to the speed the stream is being read at.
Streams in JS are handled using two interfaces: ReadableStream and WriteableStream. 
ReadableStream is result of getting response from external resource for example using Fetch function (response body returned by a successful fetch request can be exposed as a ReadableStream, and you can then read it using a reader created with ReadableStream.getReader(), cancel it with ReadableStream.cancel(),...). More complicated uses involve creating your own stream using the ReadableStream() constructor, for example to process data inside a service worker.
WriteableStream is for working with stream that allow writing to it. It has a writer (like Reader for ReadableStream) which help to write in it. 
Consider that both of ReadableStream (this has constructor too I think?!) and WriteableStream are interface. We also have other transform stream for working with stream (for example compressing them).We usually create streams with other constructors and methods.
1- You can easily create a ReadableStream from a blob. The Blob interface's stream() method returns a ReadableStream which upon reading returns the data contained within the blob. Also recall that a File object is a specific kind of a Blob, and can be used in any context that a blob can.
2- We have TextDecoder and TextEncoder which work with Typed array. The streaming variants of it are TextDecoderStream and TextEncoderStream. You can use them to create Binary(Uint8Array)(encoded) and string (decoded) ReadableStream.
3- Getting a response from a fetch request, its body return a binary readable stream which can be decoded using TextDecodedStream.
4- Compressing or decompressing a file is easy with the CompressionStream and DecompressionStream transform streams respectively.
You can see here a good documentation from google (web.dev)
https://web.dev/streams/








