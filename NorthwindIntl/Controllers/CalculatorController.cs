using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NorthwindIntl.ActionConstraints;
using NorthwindIntl.ValueProviders;
using NorthwindIntl.ExceptionFilter;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Redis;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Net;
using Microsoft.Extensions.Primitives;
using System.Threading;

namespace NorthwindIntl.Controllers
{
    public class CalculatorController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //This will be execute before action(synchronously)
            Console.WriteLine("I executed before starting action");
        }

        // [ActionName("CalculateByMethod")]
        public int CalculateA(string id, int a, int b)
        {
            return id switch
            {
                "Add" => a + b,
                "Multiply" => a * b,
                "Divide" => a / b,
                "Reminder" => a % b,
                "Minus" => a - b,
                "Length" => (int)Math.Sqrt(a ^ 2 + b ^ 2),
                _ => 0
            };
        }

        // [ActionName("Add")]
        // [HttpGet("Calculate({a:int},{b:int})")]
        public object Calculate(int a, int b)
        {
            // throw new Exception("This is a test exception");
            var dataToken = HttpContext.GetEndpoint().Metadata.GetMetadata<IDataTokensMetadata>();
            var foo = dataToken.DataTokens["Foo"] as string;
            return new
            {
                Foo = "foo",
                Sum = a + b,
            };
        }

        // [CustomAuthorization]
        // [HttpPost]
        // public JsonResult FormSubmit(IFormFileCollection files) {
        //     foreach (var file in files)
        //     {
        //         Console.WriteLine(file.FileName);
        //     }
        //     return Json(new {
        //         success=true,
        //     });
        // }

        // public JsonResult Process([CustomHtmlEncode] string html) {
        //     return Json(new {
        //         encoded=html
        //     });
        // }

        // public IActionResult InlineValidate(string id) {
        //     var valid=TryValidateModel(id);
        //     if (valid) {
        //         return Ok();
        //     } else {
        //         return BadRequest();
        //     }

        // }

        public IActionResult Subscribe([Required, EmailAddress] string email)
        {
            if (ModelState.IsValid)
            {
                return Ok();
            }
            return BadRequest();

        }

        public IActionResult CheckMarriedValidation(Person person)
        {
            Console.WriteLine("I am action");
            if (ModelState.IsValid)
            {
                return new XMLResult(person);
            }
            Console.WriteLine(ModelState.GetFieldValidationState("person"));
            return Json(new { success = false });

        }

        public FileStreamResult StreamSong(string name)
        {
            var path = System.IO.Path.Combine(Environment.CurrentDirectory, "wwwroot/sounds/1.mp3");
            var stream = System.IO.File.OpenRead(path);
            return new FileStreamResult(stream, "Audio/mp3");
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            Console.WriteLine("I executed after finishing action");
            // This will execute after action
        }

        [CustomExceptionFilter]
        public void ExceptionPage()
        {
            throw new Exception("This is a test exception for checking Exception Filters!");
        }

        [ResponseCache(CacheProfileName = "MyCacheProfile")]
        public JsonResult Cache()
        {
            return Json(new
            {
                Message = "I am cached using cache profile",
                CurrentTime = DateTime.UtcNow,
            });
        }

        [ResponseCache(Duration = 30)]
        public JsonResult CacheWithoutProfile()
        {
            return Json(new
            {
                Message = "I am cached without using cache profile",
                CurrentTime = DateTime.UtcNow,
            });
        }

        public JsonResult Cookie()
        {
            HttpContext.Response.Cookies.Append("username", "alfi", new CookieOptions
            {
                Domain = "localhost",
                Expires = DateTimeOffset.Now.AddMinutes(2),
                Secure = false,
                HttpOnly = true,
                Path = "/",
            });
            return Json(new
            {
                Message = "I am cached without using cache profile",
                CurrentTime = DateTime.UtcNow,
            });
        }

        public JsonResult Session()
        {
            var sessionID = HttpContext.Session.Id;
            var containsKey = HttpContext.Session.TryGetValue("key", out byte[] sessionKey);
            if (containsKey)
            {
                return Json(new
                {
                    sessionID,
                    sessionKey = sessionKey.ToString()
                });
            }
            else
            {
                HttpContext.Session.SetString("key", "Sample Value");
                return Json(new
                {
                    sessionID,
                    Message = "Session is not set yet!",
                });
            }
        }

        // // Here we created a Change Token from a Cancellation token just for learning purpose
        // public static CancellationTokenSource cts = new CancellationTokenSource();
        // public static IDisposable changeToken = ChangeToken.OnChange(() => { var changeToken = new CancellationChangeToken(cts.Token); return changeToken; }, () => { Console.WriteLine("I used a Change Token right now"); });

        public IActionResult MemoryCache([FromServices] IMemoryCache cache)
        {

            Person person = new Person
            {
                FullName = "Ali",
                Married = true,
                PartnerName = "Fateme",
            };

            {
                // // We can use this pattern too
                var p = cache.GetOrCreate("person", cachEntryOptions =>
                  {
                      //   cachEntryOptions.ExpirationTokens.Add(1);
                      cachEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(5);// One of the best expiration setting is combing absolute expiration with sliding to 1- Ensure the data freshness 2- Ensure to not cache non-frequesnt contents
                      // This is a post eviction delegate which fired after removing object from cache!
                      PostEvictionDelegate del = (key, val, reason, state) =>
                            {
                                Console.WriteLine("I evicted/removed from cache!");
                                Console.WriteLine(reason.ToString());
                                Console.WriteLine(state);
                            };
                      cachEntryOptions.RegisterPostEvictionCallback(del, "Testing");
                      // since we have a delegate here we describe it in bottom of this page
                      //   cachEntryOptions.Size = 1;// Same As following line
                      //   cachEntryOptions.SetSize(1);// This is a unit-less size which apply if we applied a limit on total size of memory cache other wise does not apply. 
                      // Total Size of Memory cache should be defined when adding cache.
                      // Usually we define two cache one which is singleton sized memory cache which used for managing size and the other which uses shared memory and do not need size.
                      // To do that create a class implementing IMemoryCache and Register it as singleton and use it in case of sizing
                      return person; // This is what cached!
                  });
                // var p=await cache.GetOrCreateAsync();// We can also ue an async version of Creation which create cache item async (for example as Task or async-await structure)
                return Json(new
                {
                    FromCache = true,
                    Person = p,
                });

            }

            // var cached = cache.TryGetValue("person", out Person p);// IMemoryCach caches the cache item as an Object so we can return it in that format with all the method it suggest. for example we can use cache.Get<Person>("person); Similary for set, altough it can infer the type itself when creating the cache item
            // if (cached)
            // {
            //     return Json(new
            //     {
            //         FromCache = true,
            //         Person = p,
            //     });
            // }
            // else
            // {

            //     cache.Set<Person>("person", person);
            //     return Json(new { FromCache = false, });
            // }


        }

        public IActionResult DistributedCache([FromServices] IDistributedCache cache)
        {
            Person person = new Person
            {
                FullName = "Ali",
                Married = true,
                PartnerName = "Fateme",
            };

            var cachedAsString = cache.GetString("person");

            if (!string.IsNullOrEmpty(cachedAsString))
            {
                Person p = JsonSerializer.Deserialize<Person>(cachedAsString);

                return Json(new
                {
                    FromCache = true,
                    Person = p,
                });
            }
            else
            {
                var personAsString = JsonSerializer.Serialize(person);
                cache.SetStringAsync("person", personAsString);
                return Json(new { FromCache = false, });
            }


        }

        public JsonResult SaveTempData()
        {
            var person = TempData;
            return Json(person.Count);
        }


        // [Host("localhost","127.0.0.1")]
        // public IActionResult Local() {
        //     return NoContent();
        // }

        // [Host("0:8080")]
        // public IActionResult Port80() {
        //     return NoContent();
        // }

        public IActionResult TestViewComponent()
        {
            return ViewComponent("My", new { p1 = 4, p2 = true }); // return view component from action
        }

    }
}


// Delegate
// Delegate is a term which used like Class, Enume and Struct and it is a refrence type definition.
// Working with Delegate include Three step (some times one or two step may had already done). This step include following:
// 1- Declaring a Delegate Type (like declaring a class for example!) and its instances and initialize them
// 2- Initialize delegateType instances to a (or multiple) methods (this is done in step 1 when creating insances usually)
// 3- Using the delegate Type and instances

// Step 1: Declaring a Delegate Type and its instance
// To declar a Delegate Type we can do that inside the namespace or inside a class (altough it is accessible outside of class if it is public). for example we can define a Delegate Type with name MyDelegate inside namespace as follows:
// public delegate void MyDelegate(string msg);
// This is all should be done. Here we define a Delegate with name MyDelegate (Or better to say we have a Type with name MyDelegate which is a delegate) which take an string and return nothing.
// we can use this type (MyDelegate) like other type with new constructor or let the compiler to cast the type itself. This type can be used as Type in classes as property type,field type, method parameters type, method return type.
// consider that this type can work with typeof and instances of it does have to string or gettype methods.
// now to declare an instance with that type (MyDelegate) we have the following ways (There is other ways too)
// MyDelegate log; // Here we do not target any method 
// MyDelegate log=new MyDelegate(myMethod); // Here we have a method named myMethod (can be a class method for example)
//  var log=new MyDelegate(myMethod); // Here we let the compiler to infere the type
// MyDelegate log=myMethod; // Without using the new keyword
// MyDelegate log=(msg)=>console.WriteLine(msg); // Using Lambda Experssion
// MyDelegate anynomousLog=delegate(string message) {Console.WriteLine(message);}; // Using anynomous methods
// MyDelegate anotherLog=log; // Here we assined other delegate instance to this delegate instance 
// Another interesting thing about delegates is that we can do -,+,-= and += with their instance. for example consider this:
// MyDelegate delegateCompound;
// delegateCompound=log;
// delegateCompound+=anotherLog;
// MyDelegate newLog=delegateCompound-log;
// newLog-=anotherLog;
// In the above example myMethod can be any methods with this signature:(sting x)=>void

// Step 3- Using the Delegate Types and their instance
// As it said in previous steps we can use delegate type inside class and methods and may be other places. for example:
// public class MyClass {
// public MyDelegate _myInstance; // Use as field or property type
// public MyClass(MyDelegate myInstance) { // use as constructor parameter type
// _myInstance=myInstance;
// }
// public void doSomething(string message, MyDelegate del) {
// del.Invoke(message);
// _myInstance+=del;
// return _myInstance; // even we can return the instance here but we should use MyDelegate as return type if its the case
// }
// }
// As you see we can call instances of MyDelegate Type using Invoke. We can call it directly as del(message) too.
// Delegates can be defined Genric too. for example we have:
// public delegate T MyGenericDelegate<T>(string msg, T obj);// Generic class definition
// var myInstance = new MyGenericDelegate<int>((string message, int x) => { return x + message.Length; });
// Normally we do not define new delegate Types and use built in types which are as follow:
// Predicates which take an input of type T and return boolean
// Action: this get as many as you like parameters and do not return any thing.
// Func: this get as many as you like parameters and do return output with type T.
// for Action and Func input parameters type are arbitary and can be different.
// There is concepts which are connected to delegate including: 
// Events: which is a delegate Type with different that it have to use use (+=,-=) to not remove other handlers I think
// and used for communication between components. Also An event is raised but cannot be passed as a method parameter. 
// Anynomous methods: They are a way to define delegate instances value 

// Events
// As it said that is a tool for communctation between components in dotnet. It is based on Observer Design Pattern. Here, there is
// One Publisher who raises the event and one or multiple Subscribers that catch that with event handler.
// To declare an event we should first declare a delegate type and then create a variable with that delegate type (An instance of it) using event keyword in the Publisher.
// // Here the delegate specify signature of event handlers in Subscriber class For example consider this Publisher class:
// public delegate void Notify(); // delegate
// public class ProcessBusinessLogic // This the Publisher
// {
//     public event Notify ProcessCompleted; // Event itself
//     protected virtual void OnProcessCompleted()
//     {
//         // This the method that should used for raising the event here and on derived classes.
//         // Derived classes should override this method if they want to change the logic for event raising but at the end they should call this method to ensure that registered handlers receives the event
//         // This method should have the signature to send event data and event publisher which we describe in next 
//         ProcessCompleted?.Invoke();
//     }

//     public void StartProcess()
//     {
//         // Do some stuf
//         // For raising the event instead of using invoke directly, use the OnEvent method which designed for raising the event
//         OnProcessCompleted();
//     }
// }

// // To Register a Subscriber class with this event we should add event handlers with delegate specified signature to the event 
// public class SomeClass
// {
//     // define event handler as a method with delegate signature and this naming convention
//     public void bl_ProcessCompleted()
//     {
//         // Handle event
//     }

//     // To subscribe to the even use an instance of Publisher (Usually service injection but here
//     // with constructor since we ourselves making to raise the even primarly) and subscribe to event.
//     public void SomeMethod(int x)
//     {
//         var bl = new ProcessBusinessLogic();
//         bl.ProcessCompleted += bl_ProcessCompleted; // This subscribe to event listener
//         bl.StartProcess();
//         // do some stuf
//     }

// }

// Built-in EventHandler and EventHandler<TEventArgs> in dotnet
// Typically, any event should include two parameters: the source of the event and event data.
// Use the EventHandler delegate for all events that do not include event data.
// Use EventHandler<TEventArgs> delegate for events that include data to be sent to handlers.
// Using them we should declare event itself as:
// public event EventHandler ProcessCompleted; 
// // And the standard event raiser as following to send the data to handlers
// protected virtual void OnProcessCompleted(EventArgs e)
// {
//     ProcessCompleted?.Invoke(this, e);
// }
// Here we use this to send the source of event raiser to handlers. Also event arguments as e sent to event handlers.
// If we have not any event data we can use EventArgs.Empty to send it empty. The following show the event handlers signature:
// If you want to pass more than one value as event data, then create a class deriving from the EventArgs base class (I think this is wrong! But as I search Apparabtly it gives more features if  we inherit from EventArgs and use the standard naming which end in).
// Or use a Poco class for T type and use the T type in event handler in Subscriber class as input type. 
// event handler
// public void bl_ProcessCompleted(object sender, EventArgs e)
// {
//     Console.WriteLine("Process Completed!");
// }


// ChangeToken Static class
// When a change happens, for example in file system or in memory cache items or a cancellation token happened, we may be interested to do something as callback. 
// Microsoft Provide a mechanism for this cases which needs Change Token Static class. This class is static and we have an interface IChangeToken which is related to this. 
// This class does not implement that interface but help to generate an implementation of that interface. There is multiple implemntation of that interface which can be used according to our use case.
// The interface has 2 bool property and a method as follows:
// bool HasChanged=> It is false by default. If it became true a change happened
// bool ActiveChangeCallback=> if this is true, change callback wich register for the token implementation called by setting HasChange to true, otherwise the app should implement a mechansim for calling the call back after setting HasChange to true.
// RegisterChangeCallback(Action<Object>,Object)=> this return an IDisposable. It register an action callback which get an object as input. This run automaticly if ActiveChangeCallback be true. Otherwise app should run call back by continuously checking (pool) HasChange property
// The static class ChangeToken has only one Generic method (which has nongeneric version too)
// OnChange(Func<IChangeToken>,Action)=> This return an IDisposable. the first parameter is a Func which return one IChangeToken using an implementation of IChangeToken and the action is an action callback which called after setting the HasChange Property of the returned token to true. 
// OnChange<T>(Func<IChangeToken>,Action<T>,T)=> This is above version except we pass a T object to the callback action as third parameter.

// We have multiple implementation of IChangeToken
// As I said before ChangeToken is static and it is not an implementation of IChangeToken. We should use other implementation of it in the OnChange method of ChangeToken static class.
// Here we describe some of this IChangeToken Implementation:
// 1- CancellationChangeToken: We create a IChangeToken from CancellationToken. The HasChange get true when we have a cancellation.
// var cancellationToken=new CancellationTokenSource().Token;var changeToken=new CancellationChangeToken(cancellationToken);
// 2- PollingFileChangeToken: This create a change token which monitors file changes and when file changes the HasChange gets true. This token watch file change using watch method of file provider which checks the file change every 4 seconds ! 
// var changeToken=new PollingFileChangeToken (System.IO.FileInfo fileInfo);
//  It also can create from the Watch method of file provider as:
// var fileInfo = new FileInfo(@"C:\Some\File.txt"); var fileProvider = new PhysicalFileProvider(fileInfo.DirectoryName); var changeToken=fileProvider.Watch(fileInfo.Name);
// 3- PollingWildCardChangeToken: Similar to above except that it check a wildcard set of files. Its constructor is as below:
// var changeToken=new PollingWildCardChangeToken (string root, string pattern); where root is root of file system and pattern is the pattern which should considered
// 4- NullChangeToken: This does not actually do any thing. It is return by file provifer if the file was not found
// 5- ConfigurationChangeTokenSource: This is used to controll changes in configuration and it create the change token as below:
// var ccts = new ConfigurationChangeTokenSource<MyOptions>(this.Configuration); //MyOption is the options we try to monitor here
// var changeToken=ccts.GetChangeToken();
// 6- CompositeChangeToken: We can combine multiple change token to create a token which set it HasChange=true if one of the token has changed
// var changeToken=new CompositeChangeToken(IReadOnlyList<IChangeToken> );

