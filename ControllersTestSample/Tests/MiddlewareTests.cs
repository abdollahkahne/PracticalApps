using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ControllersTestSample.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Tests
{
    public class MiddlewareTests
    {
        [Fact]
        public async Task UseGlobalHeader__ReturnNotFoundStatusCode()
        {
            // we build and start a test host (an in memory test server). It include following Steps:
            // 1- create a host builder
            // 2- Configure we host builder which include steps including configure service and configure and also more settings. Here we determine to use test server
            // 3- Start the host which includ building it and the starting it

            // why test server instead of real host and server:
            // 1- Middleware can be tested in isolation with TestServer. It allows you to:
            //     Instantiate an app pipeline containing only the components that you need to test.
            //     Send custom requests to verify middleware behavior.

            // 2- It has Advantages:
            //     Requests are sent in-memory rather than being serialized over the network. This avoids additional concerns, such as port management and HTTPS certificates.
            //     Exceptions in the middleware can flow directly back to the calling test.
            //     It's possible to customize server data structures, such as HttpContext, directly in the test.

            using (var host = await new HostBuilder().ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder.UseTestServer()
                // .ConfigureServices(services => services.AddMyService()) // This step is equal to Configure Service method in Startup
                .Configure(app => app.UseGlobalHeader()); // This step is equal to Configure method in startup
            }).StartAsync())
            {
                // create an HttpClient that allows us to send HTTP requests to the in-memory server
                var response = await host.GetTestClient().GetAsync("/"); // This return not found response but with headers
                Console.WriteLine(response.Headers);

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task UseGlobalHeader__ReturnGeneralHeader()
        {
            using (var host = new HostBuilder().ConfigureWebHost(builder =>
            {
                builder.UseTestServer().Configure(app => { app.UseGlobalHeader(); });
            }).Start())
            {
                // Act 
                var response = await host.GetTestClient().GetAsync("/?and=query");
                var Header = response.Headers.GetValues("global-header");
                // Console.WriteLine(Header.ToString());

                // Assert

                Assert.NotEmpty(Header);
            }
        }

        [Fact]
        public async Task UseGlobalHeader_SettingContextDirectly_ReturnGeneralHeader()
        {
            // As an alternative to create test host directly in Host Builder configuration we can first configure it as normal and then
            // create a test server for it. For example
            // var hostBuilder = new HostBuilder().ConfigureWebHost(builder =>
            // {
            //     builder.Configure(app => { app.UseGlobalHeader(); });
            // });
            // var server = new TestServer(hostBuilder as WebHostBuilder);


            using (var host = new HostBuilder().ConfigureWebHost(builder =>
            {
                builder.UseTestServer().Configure(app => { app.UseGlobalHeader(); });
            }).Start())
            {
                // Arrange (Continue)
                var server = host.GetTestServer(); // Do more config on server
                server.BaseAddress = new Uri("https://example.com/A/path");
                // HttpContext initialContext =
                // Act (httpContext is context that show changes due to middleware and ctx is what we start with it before going to middleware)
                // This way even we can add Features and Items to Http Context which are server-side
                // SendAsync permits direct configuration of an HttpContext object rather than using the HttpClient abstractions. Use SendAsync to manipulate structures only available on the server, such as HttpContext.Items or HttpContext.Features.
                var httpContext = await server.SendAsync(ctx =>
                {
                    ctx.Request.Method = HttpMethods.Post;
                    ctx.Request.Path = "/and/file.txt";
                    ctx.Request.QueryString = new QueryString("?and=query");
                });
                var Header = httpContext.Response.Headers.TryGetValue("global-header", out var header);
                // Console.WriteLine(header.ToString());

                // Assert
                Assert.True(Header);
            }
        }

        [Fact]
        public async Task UseGlobalHeader_DontUseTestServer_ReturnGeneralHeader()
        {
            // Arrange
            var expected = new StringValues("This header added by middleware");
            var httpContext = new DefaultHttpContext(); // We can use this as mock version of HttpContext
            var feature = new Mock<IHttpResponseFeature>();
            feature.Setup(f => f.HasStarted).Returns(true);
            httpContext.Features.Set<IHttpResponseFeature>(feature.Object);
            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new GlobalHeaderMiddleware(next);


            // Act
            await middleware.InvokeAsync(httpContext);
            var response = httpContext.Response;
            var isLocal = response.Headers.TryGetValue("global-header", out var actual);


            // Assert
            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)response.StatusCode);
            Assert.True(isLocal);
            Assert.Equal(expected, actual);
        }

    }

    public class RequestCookieCollection : IRequestCookieCollection
    {
        private readonly Dictionary<string, string> _cookies;

        public RequestCookieCollection(Dictionary<string, string> cookies)
        {
            _cookies = cookies;
        }

        public string this[string key] => _cookies[key];

        public int Count => _cookies.Count;

        public ICollection<string> Keys => _cookies.Keys;

        public bool ContainsKey(string key)
        {
            return _cookies.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _cookies.GetEnumerator();
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        {
            return _cookies.TryGetValue(key, out value);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


}

// To make serviceProvider usually the following done:
// 1- create service collection
// 2- Add Services to service collection
// 3- Build ServiceProvider from service collection
// 4- Assign Service Provider to Http Context Request Services and use them

// To Make a controller you can do multiple ways:
// 1- for example create its dependencies using Mock and then make a new instance of it=> var controller=new HomeController(mockDependency);
// 2-build Http Context and determine its service provider and then make an instance of controller context and
// make controller using constructor of it and set its context (You can set it directly too from controller.ControllerContext or Controller.HttpContext)
// var serviceProvider = new SeviceCollection().AddSingleton<IMyService, MyService>().BuildServiceProvider();
// var httpContext=new DefaultHttpContext();httpContext.RequestServices=serviceProvider;
// var controllerContext=new ControllerContext(httpContext); controller.ControllerContext=controllerContext;


// To make HttpContext usually use DefaultHttpContext as Mock and then add custom properties to it
// To Build the user property for example we should do the following
// 1- Build ClaimsIdentity=> new ClaimsIdentity(new Claims[] {new Claim(ClaimTypes.Name,"username"),...});
// 2- Build ClaimsPrincipal from ClaimsIdentity=> var user=new ClaimsPrincipal(claimsIdentity);
// 3- Add it to Default HttpContext=> var context=new DefaultHttpContext();context.User=user;

// To Build Features Property You can use Generic Set method from HttpContext Features Property itself or use DefaultHttpContext Constructor
// 1- Build Feature Collection; var features=new FeatureCollection(); features.Set<FeatureType>(featureMock Or FeatureImplementation)
// 2- Add it to Http Context through Constructor; var httpContext=new DefaultHttpContext() {Features=features}

// To add Cookies to HttpContext we can use IRequestCookieFeature similar to above line. The only difference is in Setting the feature
// var cookiesDictionary=new Dictionary<string,string>() {{"username","dummy"}} ;
// var cookies=new RequestCookieCollection(cookiesDictionary);
//...; features.Set<IRequestCookiesFeature>(new RequestCookiesFeature(cookies));...
// since Request Cookies Collection class is internal we should re-implement here in Test project as above:

// To add Form Data to Http Context we use IFormFeature
// 1- add Dictionary
// var formData = new Dictionary<string, StringValues> {
//     {"name","James"},
//     {"email","james@yahoo.com"}
// };
// 2- Make Form Collection
// var form=new FormCollection(formData); // we can add files here
// 3- Make Form Featur
// var formFeature = new FormFeature(form);
// 4- Set IForm Feature Implemetation
// ...; features.Set<IFormFeature>(form);...

