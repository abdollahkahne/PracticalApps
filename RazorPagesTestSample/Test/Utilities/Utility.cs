using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Src.Data;
using Src.Pages;

namespace Test.Utilities
{
    public static class Utility
    {
        // We use db context options in constructor to build the db context
        // so to test db context we need this option
        public static DbContextOptions<AppDbContext> TestDbContextOptions()
        {
            // create service provider (The service provider is responsible for building in Memory database)
            var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

            // serviceProvider can assigned to Http Context Request Services Too if we need it.
            // Here we need it only to distinguish between different test Service Provider
            // The process of building Service Provider in ASP.Net Host is through Progam.cs or host building.
            // There it will create a service collection first and inject all services to it from startup class or directly using ConfigureService Method and then
            // Build a service provider from it and give it to http context and also use it in dependency injection 

            // create Db Context Option which uses this Service Provider
            // This way db context option created every time that it needs by a test
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("InMemory")
            .UseInternalServiceProvider(serviceProvider).Options;
            return options;

            // why we dont create the option directly in tests itself  to use it?
            // var option = new DbContextOptionsBuilder<AppDbContext>()
            // .UseInMemoryDatabase("InMemory").Options;
            // The problem with this approach is that each test receives the database in whatever state the previous test left it (Since Service Provider for all tests is same regarding to using test server as entry point).
            // This can be problematic when trying to write atomic unit tests that don't interfere with each other.
            // To force the AppDbContext to use a new database context for each test, supply a DbContextOptions instance that's based on a new service provider (which created too). 
        }
        public static IndexModel TestIndexModel(AppDbContext db)
        {
            // Setup Contexts like PageContext, HttpContext, ActionContext,RouteData,ModelState, PageActionDescriptor and...
            var httpContext = new DefaultHttpContext();// we can use DefaultHttpContextFactory too and the create this
            var routeData = new RouteData();
            var pageActionDescriptor = new PageActionDescriptor();
            var modelState = new ModelStateDictionary();
            var actionContext = new ActionContext(httpContext, routeData, pageActionDescriptor, modelState);
            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            // var tempData = new TempDataDictionaryFactory(Mock.Of<ITempDataProvider>()).GetTempData(httpContext);

            var pageContext = new PageContext(actionContext) { ViewData = viewData }; // This is all the above objects container and we insert it in PageModel Constructor

            // inject Db Context to Index Page Model and also PageContext,Url Helper, Temp Data
            var pageModel = new IndexModel(db) { PageContext = pageContext, Url = new UrlHelper(actionContext), TempData = tempData };
            return pageModel;
        }
    }
}