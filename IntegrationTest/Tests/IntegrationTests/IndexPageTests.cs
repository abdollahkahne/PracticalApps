using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Src;
using Src.Data;
using Src.Services;
using Tests.Helpers;
using Xunit;

namespace Tests.IntegrationTests
{
    public class IndexPageTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public IndexPageTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            var clientOptions = new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
            };
            _client = _factory.CreateClient(clientOptions);
        }

        [Fact]
        public async Task DeleteAllMessagesPostHandler_IndexPageIsCurrentPage_ReturnRedirectToRoot()
        {
            // Arrange
            var indexResponse = await _client.GetAsync("/");
            indexResponse.EnsureSuccessStatusCode();
            var htmlDoc = await HtmlHelpers.GetDocumentAsync(indexResponse);
            var form = htmlDoc.QuerySelector("#messages") as IHtmlFormElement;
            var button = htmlDoc.QuerySelector("#deleteAllBtn") as IHtmlElement;

            // Act
            var response = await _client.SubmitFormAsync(form, button, new Dictionary<string, string>());
            // response.EnsureSuccessStatusCode();

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task DeleteAMessagePostHandler_IndexPageIsCurrent_ReturnRedirectToRoot()
        {
            var messageId = 1;
            // Create a new factory (We want to have the database assigned to this factory should be initialized)
            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(async services =>
                {
                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var scopedServicesProvider = scope.ServiceProvider;
                        var db = scopedServicesProvider.GetRequiredService<AppDbContext>();
                        var logger = scopedServicesProvider.GetRequiredService<ILogger<IndexPageTests>>();
                        try
                        {
                            await db.DeleteAllMessagesAsync();
                            db.Initialize();
                        }
                        catch (System.Exception ex)
                        {
                            logger.LogError(ex, "Something is wrong: {message}", ex.Message);

                        }
                    }
                });
            });
            // we create new client from the factory
            var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var indexPageResponse = await client.GetAsync("/"); // This makes the cookie related to Antiforgery handled by HttpClient  but in order to send it with Post Request we should parse it using AngleSharp for example or we can do it directly too from Headers.GetCookies()??
            var indexPageHtml = await HtmlHelpers.GetDocumentAsync(indexPageResponse);
            var form = indexPageHtml.QuerySelector("#messages") as IHtmlFormElement;
            var deleteButtonId = $"#deleteBtn{messageId}";
            var submitButton = indexPageHtml.QuerySelector(deleteButtonId) as IHtmlElement;

            // Act
            var response = await client.SubmitFormAsync(form, submitButton, new Dictionary<string, string>());

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/", response.Headers.Location.OriginalString);

        }

        [Fact]
        public async Task AddMessagePostHandler_InvalidModel_ReturnSuccess()
        {
            // Given
            var indexPageResponse = await _client.GetAsync("/");
            indexPageResponse.EnsureSuccessStatusCode();
            var indexPagDocument = await HtmlHelpers.GetDocumentAsync(indexPageResponse);
            var form = indexPagDocument.QuerySelector("#addMessage") as IHtmlFormElement;
            var button = indexPagDocument.QuerySelector("#addMessageBtn") as IHtmlElement;
            var formValues = new Dictionary<string, string>();
            formValues.Add("Message.Text", string.Empty);

            // When
            var response = await _client.SubmitFormAsync(form, button, formValues);

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            response.EnsureSuccessStatusCode();
            Assert.Null(response.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task AddMessagePostHandler_ValidModel_ReturnRedirectToRoot()
        {
            //Given (Approach 2 to clean database to initial state)
            var serviceProvider = _factory.Services;
            // Create an scoped version of db Context since EFCore needs scoped injection
            using (var scope = serviceProvider.CreateScope())
            {
                var scopedServicesProvider = scope.ServiceProvider;
                var db = scopedServicesProvider.GetRequiredService<AppDbContext>();
                var logger = scopedServicesProvider.GetRequiredService<ILogger<IndexPageTests>>();
                try
                {
                    await db.DeleteAllMessagesAsync();
                    db.Initialize();
                }
                catch (System.Exception ex)
                {

                    logger.LogError(ex, "Something is wrong:{message}", ex.Message);
                }

            }

            // Get Index Page 
            var indexResponse = await _client.GetAsync("/");
            indexResponse.EnsureSuccessStatusCode();
            var indexHtml = await HtmlHelpers.GetDocumentAsync(indexResponse);
            var form = indexHtml.QuerySelector("#addMessage") as IHtmlFormElement;
            var submitBtn = indexHtml.QuerySelector("#addMessageBtn") as IHtmlElement;
            var newMessage = new Dictionary<string, string>();
            newMessage.Add("Message.Text", "This is a tes message");


            //When
            var response = await _client.SubmitFormAsync(form, submitBtn, newMessage);

            //Then
            // Response is Redirect to Root
            // To Check the response, we can allow redirect in client and then check the list of messages in Inedx.html
            // Checking list of messages from database should be done at unit test level for controller action/razor page handler 
            // Checking Property Binding is a unit test level (Property Message should be bind to form values)
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task AddMessagePostHandler_InvalidModel_ReturnOK()
        {
            // Given
            var indexPageResponse = await _client.GetAsync("/");
            indexPageResponse.EnsureSuccessStatusCode();
            var indexPagDocument = await HtmlHelpers.GetDocumentAsync(indexPageResponse);
            var form = indexPagDocument.QuerySelector("#addMessage") as IHtmlFormElement;
            var button = indexPagDocument.QuerySelector("#addMessageBtn") as IHtmlElement;
            var formValues = new Dictionary<string, string>();
            formValues.Add("Message.Text", new string('x', 400)); // A message with more than 200 chars

            // When
            var response = await _client.SubmitFormAsync(form, button, formValues);

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            response.EnsureSuccessStatusCode();
            Assert.Null(response.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task AnalyseMessagesPostHandler_ReturnRedirect()
        {
            //Given (we are in index page)
            var indexResponse = await _client.GetAsync("/");
            indexResponse.EnsureSuccessStatusCode();
            var htmlPage = await HtmlHelpers.GetDocumentAsync(indexResponse);
            var form = htmlPage.QuerySelector("#analyze") as IHtmlFormElement;
            var button = htmlPage.QuerySelector("#analyzeBtn") as IHtmlElement;
            var formValues = new Dictionary<string, string>();

            //When (when we press Analyse button)
            var response = await _client.SubmitFormAsync(form, button, formValues);

            //Then (Page Redirect to Index Page again)
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task GetIndexPage_QuoteServiceImplementedByTest_TheQuouteInputeExist()
        {
            //Given (we remove current implementation of IQuoteService with TestQuoteService)
            // since we want to change services (in above case we only used it and does not need new factory or client), we need a new factory and client so we should use WithWebHostBuilder
            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var quoteService = services.SingleOrDefault(sd => sd.ServiceType == typeof(IQuoteService));
                    services.Remove(quoteService);
                    services.AddScoped<IQuoteService, TestQuoteService>();
                });
            });
            var client = factory.CreateClient();

            //When (we get index page)
            var response = await client.GetAsync("/");
            var htmlResponse = await HtmlHelpers.GetDocumentAsync(response);
            var quoteElement = htmlResponse.QuerySelector("#quote");
            string quote = null;
            if (quoteElement.HasAttribute("value"))
            {
                quote = quoteElement.GetAttribute("value");
            }

            //Then (the quoute is there with our specified value)
            Assert.Equal(TestQuoteService.quote, quote);
        }

    }

    internal class TestQuoteService : IQuoteService
    {
        public const string quote = "This is a test Quote which we simplify its implemetation using test class";
        public Task<string> GenerateQuote()
        {
            return Task.FromResult<string>(quote);
        }
    }
}
