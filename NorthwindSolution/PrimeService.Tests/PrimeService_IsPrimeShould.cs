using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prime.Services;

namespace Prime.UnitTests.Services
{
    [TestClass]
    public class PrimeService_IsPrimeShould
    {
        private readonly PrimeService _primeService;

        public PrimeService_IsPrimeShould()
        {
            _primeService = new PrimeService();
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Console.WriteLine("Class Initialized");
            // Here we can do things similar to constructor!
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Console.WriteLine("Class Cleaned Up!");
            // This do at the destruction time and Grravage Cikkection
        }

        [TestInitialize]
        public void TestInitialize() { }

        [TestCleanup]
        public void TestCleanup() { }

        [TestMethod]
        [Ignore("Included in other test because of similar behaviour")]
        public void IsPrime_InputIs1_ReturnFalse()
        {
            bool result = _primeService.IsPrime(1);

            Assert.IsFalse(result, "1 should not be prime");
        }

        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(1)]
        public void IsPrime_ValuesLessThanTwo_ReturnFalse(int value)
        {
            // act (arrange done in constructor)
            var result = _primeService.IsPrime(value);

            // assert
            Assert.IsFalse(result, $"{value} should not be prime");
        }
    }
}