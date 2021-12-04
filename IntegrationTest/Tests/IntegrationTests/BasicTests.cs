using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Src;
using Xunit;

namespace Tests.IntegrationTests
{
    // When we use Fixture the class considered a test class but we should also apply the methods that are test using related attribute like Fact and Theory
    public class BasicTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    // we used IClass Fixture to build an instance of the TFixture Class and use it in constructor. If it is disposable (Which is true in case TFixture is WebApplicationFactory) it dispose it at the end too.
    // This class should have a contructor which take TFixture class as parameter and use it as it required
    // TFixture class should have a parameter-less constructor but consider the following too:
    // classes without constructors are given a public parameterless constructor by the C# compiler in order to enable class instantiation.
    // In a derived class, if a base-class constructor is not called explicitly by using the base keyword, the parameterless constructor of base class, if there is one, is called implicitly. 
    // WebApplicationFactory<T> has parameter-less constructor. so it called here
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public BasicTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Index")]
        [InlineData("/About")]
        [InlineData("/Privacy")]
        [InlineData("/contact")]
        public async Task GetEndPoint_UseTestServer_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            //Given (Arrange)
            var client = _factory.CreateClient();
            var expected = "text/html; charset=utf-8";
            //When (Act)
            var response = await client.GetAsync(url);
            //Then (Assert)
            response.EnsureSuccessStatusCode();
            var actual = response.Content.Headers.ContentType.ToString();
            Assert.Equal(expected, actual);
        }

    }
}