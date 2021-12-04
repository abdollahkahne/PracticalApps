using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using APIClient.DataTransferObjects;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;

namespace APIClient.Services
{
    public class HttpClientPatchService : IHttpClientServiceImplementation
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions();

        public HttpClientPatchService(HttpClient client)
        {
            _client = client;
            // set client base address and 
            // _client.BaseAddress = new Uri("http://localhost:5000/api/");
            // _client.Timeout = TimeSpan.FromSeconds(30);
            // _client.DefaultRequestHeaders.Clear();
            _options.PropertyNameCaseInsensitive = true;
        }

        public async Task Execute()
        {
            // await patchEmployee();
            await patchAsyncUsingHttpRequestMessage();
        }

        private async Task patchEmployee()
        {

            var employeeId = "021CA3C1-0DEB-4AFD-AE94-2159A8479811";
            var companyId = "3D490A70-94CE-4D15-9494-5248280C2CE3";

            var patchs = new JsonPatchDocument<EmployeeForUpdateDto>();
            patchs.Add(e => e.Name, "Jimi Karager");
            patchs.Add(e => e.Age, 43);
            patchs.Add(e => e.Position, "Manager");

            // create body of request as httpContent to send
            // We should have a body as an array and Default Serializer do not serialize as it should. serialization here is somehow different and it some type of conversion from JsonPatchDocument to Json string
            // var patchAsJson = System.Text.Json.JsonSerializer.Serialize(patchs);// Serialize using System.Text.Json (this is make bad request since the JsonPatchDocument is a class from Newtonsoft and it has special json serialization apparantly)
            var patchAsJson = JsonConvert.SerializeObject(patchs);//Serialize using NewtonSoft. We have to do this, otherwise, we get 400 bad request from our API since the patch document isn’t serialized well with System.Text.Json
            var body = new StringContent(patchAsJson, System.Text.Encoding.UTF8, "application/json-patch+json"); // This the standard content type for patch but text/json works too

            var resourceUri = $"companies/{companyId}/employees/{employeeId}";
            // use PatchAsync Shortcut method
            var response = await _client.PatchAsync(resourceUri, body);
            response.EnsureSuccessStatusCode();

            //extract data from response content and deserialize it
            var content = await response.Content.ReadAsStringAsync();
            var employee = System.Text.Json.JsonSerializer.Deserialize<EmployeeDto>(content, _options);//Deserialize using System.Text.Json (this works since the response.Content is an own-defined type!)
            // var employee = Newtonsoft.Json.JsonConvert.DeserializeObject<EmployeeDto>(content);// Deserialize using NewtonSoft
        }

        private async Task patchAsyncUsingHttpRequestMessage()
        {
            var employeeId = "021CA3C1-0DEB-4AFD-AE94-2159A8479811";
            var companyId = "3D490A70-94CE-4D15-9494-5248280C2CE3";
            var resourceUri = $"companies/{companyId}/employees/{employeeId}";
            var patchs = new JsonPatchDocument<EmployeeForUpdateDto>();
            patchs.Add(e => e.Position, "Temporary Manager");

            // create request
            var request = new HttpRequestMessage(HttpMethod.Patch, resourceUri);
            // we were using strings to create a request body and also to read the content of the response. But we can optimize our application by improving performance and memory usage with streams. 
            request.Content = new StringContent(JsonConvert.SerializeObject(patchs), System.Text.Encoding.UTF8, "application/json-patch+json");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/json"));

            // send request and get response
            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // Extract response and deserialize it
            // we were using strings to create a request body and also to read the content of the response. But we can optimize our application by improving performance and memory usage with streams. 
            var content = await response.Content.ReadAsStringAsync();
            var employee = System.Text.Json.JsonSerializer.Deserialize<EmployeeDto>(content, _options);
        }
    }
}