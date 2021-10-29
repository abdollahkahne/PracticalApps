using NUnit.Framework;
using Prime.Services;

namespace Prime.UnitTests.Services
{
    [TestFixture]
    public class PrimeService_IsPrimeShould
    {
        private PrimeService _primeService;

        [SetUp]
        public void Setup()
        {
            // This runs before every test. If we need to run it once befor the
            // Initializing Tests we can define a public class with [SetupFixture] attribute and
            // related method for setup [OneTimeSetup] and finishing [OneTimeTearDown] attributes
            _primeService = new PrimeService();
        }

        [Ignore("This is already included in LessThanTwo Scenario")]
        [Test]
        public void IsPrime_InputIsOne_ReturnFalse()
        {
            // arrange-act-assert cycle
            // arrange done in constructor or SetUp or here
            //Act
            var result = _primeService.IsPrime(1);

            //Assert
            // Assert.Pass();
            Assert.IsFalse(result, "1 (One) should not be Prime");
        }

        [Test] // This can be removed since TestCase is existed
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        public void IsPrime_InputLessThanTwo_ReturnFalse(int input)
        {
            var result = _primeService.IsPrime(input);

            Assert.IsFalse(result, $"{input} should not be Prime");
        }
    }
}