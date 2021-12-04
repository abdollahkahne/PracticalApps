using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using APIClient.DataTransferObjects;

namespace APIClient.Services
{
    public class HttpClientCancellationService : IHttpClientServiceImplementation
    {
        // We better to share cancellation token between multiple methods to cancel the request already is doing the process
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly HttpClient _client; // to prevent from generating on every request we can make it static
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions();
        //Using streams with HTTP requests can help us reduce memory consumption and optimize the performance of our app
        // Also when using stream we better to use HttpCompleteionOption.ResponseHeadersRead to completely remove buffering
        public HttpClientCancellationService(HttpClient client)
        {
            // config http Client
            _client = client;
            // _client.Timeout = new TimeSpan(0, 0, 30);
            // _client.BaseAddress = new Uri("http://localhost:5000/api/");
            // _client.DefaultRequestHeaders.Clear();

            // config serialization options
            _options.PropertyNameCaseInsensitive = true;

            // Generate cancellation token source
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task Execute()
        {
            _cancellationTokenSource.CancelAfter(100);
            try
            {
                await getCompanies(_cancellationTokenSource.Token);
            }
            catch (System.OperationCanceledException OperationCanceledException)
            {

                Console.WriteLine(OperationCanceledException.Message);
            }

            // await createCompany();
        }

        private async Task getCompanies(CancellationToken cancellationToken)
        {
            // //Create a cancellation Token Source and set it to cancel after a while
            // var source = new CancellationTokenSource();
            // source.CancelAfter(10);
            // wrapping our response inside the using directive since we are working with streams now.
            using (var response = await _client.GetAsync("companies", HttpCompletionOption.ResponseHeadersRead, cancellationToken)) // We send the cancellation token with request
            {
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStreamAsync();
                var companies = await JsonSerializer.DeserializeAsync<List<CompanyDto>>(content, _options);// Deserialize async should use to accept stream. but in case of string since it already stored in a variable it got received and we use synchronous Deserialize.
            }
        }

        private async Task createCompany()
        {
            var newCompany = new CompanyForCreationDto
            {
                Name = "Google Inc.",
                Country = "USA",
                Address = "Silicon Valley 10"
            };

            // we want to use stream for request body 
            using (var ms = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(ms, newCompany);
                ms.Seek(0, SeekOrigin.Begin);// Why? When we set the stream the position move by sequence length and if we need to read it (byte by byte for example when sending it) we should change its position
                // to its begining and if we want to attach something to it the current position is okay
                var request = new HttpRequestMessage(HttpMethod.Post, "companies");
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/json"));
                using (var body = new StreamContent(ms))
                {
                    request.Content = body;
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/json");
                    using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        var content = await response.Content.ReadAsStreamAsync();
                        var company = await JsonSerializer.DeserializeAsync<CompanyDto>(content, _options);
                    }
                }
            }
        }
    }
}