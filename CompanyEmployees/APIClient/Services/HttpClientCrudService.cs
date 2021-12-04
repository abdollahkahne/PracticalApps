using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using APIClient.DataTransferObjects;

namespace APIClient.Services
{
    // we can use similar pattern which named Typed Client. 
    // They defined as a class like here but instead of direct creation of HttpClient instance use it from Dependency Injection
    // To inject the http client instance we should register this Typed Client using AddHttpClient Method and we can add more config and delegate handler here too (In addition to what we do in constructor!!!)
    // Similarly we can add methods and specific configuration (even using delegate handler which do some changes to request messages (We have one of them in NorthwindMvc))
    // for using them we can inject them directly to our constructor and consume theme (for other versions we should inject HttpClientFactory and then create an instance from them using CreateClient method)
    public class HttpClientCrudService : IHttpClientServiceImplementation
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        public HttpClientCrudService(HttpClient client)
        {
            _client = client;
            // we have here two property types:
            // 1- Properties which really belongs to HttpClient:like BaseAddress and Timeouy
            // 2- Properties related to request  or better to say related to HttpRequestMessage 
            // The best practice is to set up the default configuration on the HttpClient instance and the request configuration on the HTTP request itself. 
            // _client.DefaultRequestHeaders.Clear(); // Clear Default Request Headers
            // _client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/json"));
            // _client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/xml"));// Accept is a collection so we can add to it multiple value
            // _client.BaseAddress = new Uri("http://localhost:5000/api/");
            // _client.Timeout = new TimeSpan(0, 0, 30);
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }
        public async Task Execute()
        {
            // await getCompanies();
            // await getCompaniesWithJSONHeader();
            await getCompaniesWithXMLHeader();
            // await createCompany();
            // await createCompanyUsingHttpRequestMessage();
            // await updateCompany();
            // await updateCompanyUsingSendAsync();
            // await deleteCompany();
            // await deleteCompanyUsingSendAsync();
        }

        // Use the HttpClient class to make HTTP requests.
        // HttpClient supports only async methods for its long-running APIs. 
        private async Task getCompanies()
        {
            // var x=await _client.GetStringAsync("companies");//Send a GET request to the specified Uri and return the response body as a string in an asynchronous operation.
            // This is a shortcut method since it create a default HttpRequestMessage and then 
            var response = await _client.GetAsync("companies"); // response is HttpResponseMessage Type and has properties like Content, RequestMessage,Header, status codes
                                                                // HttpRequestMessage which can be built or get via response.RequestMessage has properties like headers, content,method and Uri

            response.EnsureSuccessStatusCode(); // This check response.IsSuccessStatusCode and if it is false it throws an exception

            var content = await response.Content.ReadAsStringAsync(); // This is possible only we have successful response
                                                                      // GetAsync("*:)+ReadAsStringAsync()=GetStringAsync("*"): make a web request and retrieve the response. When the request returns, the task reads the response stream and extracts the content from the stream. The body of the response is returned as a String, 

            var companies = new List<CompanyDto>();
            // We decide based on Content Type (which is a header of content and not header of response itself!) on what Deserializer we should us
            if (response.Content.Headers.ContentType.MediaType == "text/json")
            {
                companies = JsonSerializer.Deserialize<List<CompanyDto>>(content, _options); // The Type is used as Generic Type of method but in xml serialization we use it for building an instance of XMLSerializer itself.
                // If we know that the content-type is json always we can do Getting the api and deserializing it together
                // var companies=_client.GetFromJsonAsync("companies",typeof(List<CompanyDto>),_options);
            }
            else if (response.Content.Headers.ContentType.MediaType == "text/xml")
            {
                var doc = XDocument.Parse(content);
                foreach (var element in doc.Descendants())
                {
                    element.Attributes().Where(x => x.IsNamespaceDeclaration).Remove();
                    element.Name = element.Name.LocalName;
                }
                var serializer = new XmlSerializer(typeof(List<CompanyDto>));
                // here first convert the document to stream helper string reader and
                // then use stream to deserialize. At the end we need type casting
                companies = serializer.Deserialize(new StringReader(doc.ToString())) as List<CompanyDto>;
            }
            else
            {
                Console.WriteLine("The response content-type not supported!");
            }

            // var companies = JsonSerializer.Deserialize<List<CompanyDto>>(content, _options); // Converting from JSON (Or any other text standards like xml) to C# objects is known as deserialization. and the reverse call Serialization (From Objects in Memory to Something transportable by network)
            // The serializer automatically ignores JSON properties for which there is no match in the target class.
        }

        private async Task getCompaniesWithJSONHeader()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "companies");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/json"));

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            //JSON Serializer is a static class. so we did not to make an instance of it
            var companies = JsonSerializer.Deserialize<List<CompanyDto>>(content, _options);
        }

        private async Task getCompaniesWithXMLHeader()
        {
            // create request message
            var request = new HttpRequestMessage(HttpMethod.Get, "companies");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/xml"));

            // send request and get response
            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // convert response content to string and then deserialize it to an object using xml serializer
            var content = await response.Content.ReadAsStringAsync(); // Content is of type HttpContent which its contents should be read using one of the methods it provided. The (HttpContent)content itself is a class.
            //Convert string to a xml document can be done using xdocument or xml reader
            // XmlReader.Create(stream);// we should suplly it an sream to read from
            var document = XDocument.Parse(content);
            // clean xmlDoc from namespace attributes
            foreach (var element in document.Descendants())
            {
                element.Attributes().Where(attr => attr.IsNamespaceDeclaration == true).Remove();
                element.Name = element.Name.LocalName;
            }

            // create serializer (we should specify its type in here)
            var serializer = new XmlSerializer(typeof(List<CompanyDto>)); // The class name should be same in both client and server!
            var companies = serializer.Deserialize(new StringReader(document.ToString())) as List<CompanyDto>;
            foreach (var item in companies)
            {
                Console.WriteLine(item.Id);
            }
        }

        private async Task createCompany()
        {
            var newCompany = new CompanyForCreationDto
            {
                Name = "Eagle IT Ltd.",
                Country = "USA",
                Address = "Eagle IT Street 289"
            };
            var body = JsonSerializer.Serialize(newCompany);

            // create request content which should have content header and content body itself
            var request = new StringContent(body, System.Text.Encoding.UTF8, "text/json");
            var response = await _client.PostAsync("companies", request);

            // Ensure response is success
            response.EnsureSuccessStatusCode();

            // extract and serialize response
            var content = await response.Content.ReadAsStringAsync();
            var created = JsonSerializer.Deserialize<CompanyDto>(content, _options); // The content in created is what we defined as third parameter in CreateAtRoute method
        }

        private async Task createCompanyUsingHttpRequestMessage()
        {
            // consider this as new company. It may provide as argument to this method for example!
            var newCompany = new CompanyForCreationDto
            {
                Name = "Hawk IT Ltd.",
                Country = "USA",
                Address = "Hawk IT Street 365"
            };
            //create request
            var request = new HttpRequestMessage(HttpMethod.Post, "companies");

            // set request accept, content-type and body
            var body = JsonSerializer.Serialize(newCompany); // content type is related to content and accept is related to response media type which is a header for request itself

            request.Content = new StringContent(body, System.Text.Encoding.UTF8);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/json");// this should set after content  or in constructor with three arguments
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml")); // what we expect to receive

            //send request
            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // extract response and deserialize it
            var content = await response.Content.ReadAsStringAsync();
            var xDocument = XDocument.Parse(content);
            foreach (var xElement in xDocument.Descendants())
            {
                xElement.Attributes().Where(att => att.IsNamespaceDeclaration).Remove();
                xElement.Name = xElement.Name.LocalName;
            }

            var serializer = new XmlSerializer(typeof(CompanyDto));
            var company = serializer.Deserialize(new StringReader(xDocument.ToString())) as CompanyDto;

        }

        private async Task updateCompany()
        {
            var companyId = "5E313E06-EB7C-4A06-B385-33B2E2673481";
            var updatedCompany = new CompanyForUpdateDto
            {
                Name = "Eagle IT Ltd.",
                Country = "USA",
                Address = "Eagle IT Street 289 Updated"
            };

            var body = JsonSerializer.Serialize(updatedCompany);
            var request = new StringContent(body, System.Text.Encoding.UTF8, "text/json");

            var response = await _client.PutAsync($"companies/{companyId}", request);
            response.EnsureSuccessStatusCode();
        }

        private async Task updateCompanyUsingSendAsync()
        {
            var companyId = "5E313E06-EB7C-4A06-B385-33B2E2673481";
            var updatedCompany = new CompanyForUpdateDto
            {
                Name = "Eagle IT Ltd.",
                Country = "USA",
                Address = "Eagle IT Street 289 Updated SendAsync"
            };

            // create request
            var request = new HttpRequestMessage(HttpMethod.Put, $"companies/{companyId}");
            request.Content = new StringContent(JsonSerializer.Serialize(updatedCompany), System.Text.Encoding.UTF8, "text/json");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var xDocument = XDocument.Parse(content);
            foreach (var xElement in xDocument.Descendants())
            {
                xElement.Attributes().Where(a => a.IsNamespaceDeclaration).Remove();
                xElement.Name = xElement.Name.LocalName;
            }

            var serializer = new XmlSerializer(typeof(CompanyDto));
            var updated = serializer.Deserialize(new StringReader(xDocument.ToString()));
        }

        private async Task deleteCompany()
        {
            var companyId = "c843d2d1-b388-4342-a681-e4aa9f24e590";

            // delete async
            var response = await _client.DeleteAsync($"companies/{companyId}");
            response.EnsureSuccessStatusCode();
        }

        private async Task deleteCompanyUsingSendAsync()
        {
            var companyId = "857a38e5-62c8-4f51-88f9-9926c9414787";

            // build request message
            var request = new HttpRequestMessage(HttpMethod.Delete, $"companies/{companyId}");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/json")); // Some times it required to overwrite the defaults?According to website I read it: because some APIs return the content if something goes wrong

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
































// HttpClient
// The HttpClient class instance acts as a session to send HTTP requests.
// HttpClient instance uses its own connection pool, isolating its requests from requests executed by other HttpClient instances.
// In its constructor, we can use HttpMessageHandler to configure any pre- or post-request processing.
// To download large amounts of data (50 megabytes or more), then the app should stream those downloads and not use the default buffering. (Possibly performance problem)
// Properties of HttpClient should not be modified while there are outstanding requests, because it is not thread-safe. And to solve this use HttpClientFactory pattern. 
// The following methods are thread safe:
// CancelPendingRequests
// DeleteAsync
// GetAsync
// GetByteArrayAsync
// GetStreamAsync
// GetStringAsync
// PostAsync
// PutAsync
// SendAsync
// PatchAsync
// HttpClient is intended to be instantiated once (for example as static property and using static constructor!!) and re-used throughout the life of an application. Instantiating an HttpClient class for every request will exhaust the number of sockets available under heavy loads. This will result in SocketException errors
// HttpClient has the concept of delegating handlers that can be linked together for outgoing HTTP requests.
// Each of these handlers is able to perform work before and after the outgoing request.
// Provides a mechanism to manage cross-cutting concerns around HTTP requests, such as:
// caching
// error handling
// serialization
// logging
// One sample added as ValidateHeaderHandler in related folder and it should added as follow:
// builder.Services.AddTransient<ValidateHeaderHandler>();

// builder.Services.AddHttpClient("HttpMessageHandler")
//     .AddHttpMessageHandler<ValidateHeaderHandler>();
// More than one handler can be added to the configuration for an HttpClient
// Multiple handlers can be registered in the order that they should execute. Each handler wraps the next handler until the final HttpClientHandler executes the request (Just Like Middleware Pipeline we do a U turn calling of delegate handlers)
// IHttpClientFactory creates a separate DI scope for each handler which is separate than Request Scope too.
// The request scope value changes for each request, but the handler scope value only changes every 5 seconds (It is a Background Task). 
// HttpContext isn't thread-safe. Reading or writing properties of the HttpContext outside of processing a request can result in a NullReferenceException
// So to share per-request state with message handlers we should can use HttpRequestMessage.Properties Or use HttpContextAccessor to access HttpContext 

// HttpClientFactory? The factory manages the lifetimes of the HttpMessageHandler instances (The default handler lifetime is two minutes). IHttpClientFactory pools the HttpMessageHandler instances created by the factory to reduce resource consumption. IHttpClientFactory tracks and disposes resources used by HttpClient instances.
// So HttpClient instances can generally be treated as .NET objects not requiring disposal since HttpClientFactory Manage it.
// HttpMessageHandler: Each handler typically manages its own underlying HTTP connections. Creating more handlers than necessary can result in connection delays. Some handlers also keep connections open indefinitely, which can prevent the handler from reacting to DNS (Domain Name System) changes.
// When creating an HttpClient one HttpMessageHandler assigned to it.
// Pooling HttpMessageHandler is mechanism used by HttpClientFactory which needs some consideration:
// 1- CoockieContainer Shared between multiple Clients So we should disable automatic coockie sending to prevent this problem. In that case we can send cookie as header using http request message
// 2- To change the primary HttpMessageHandler we can use an extension method to do that:
// services.AddHttpClient("GitHubClient")
//     .ConfigurePrimaryHttpMessageHandler(() =>
//         new HttpClientHandler
//         {
//             UseCookies = false
//         });

// Header Propagation: To use some Headers from Request in our HttpClient Request we can use  Microsoft.AspNetCore.HeaderPropagation package

