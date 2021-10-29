using System;
using Prime.Services;
using Xunit;

namespace Prime.Tests.Services
{
    public class PrimeService_IsPrimeShould
    {
        private readonly PrimeService _primeService;

        public PrimeService_IsPrimeShould()
        {
            _primeService = new PrimeService();
        }

        [Fact(Skip = "This included in another test")]
        public void IsPrime_InputIsOne_ReturnFalse()
        {
            // Arrange
            // var primeService = new PrimeService();

            // Act
            var result = _primeService.IsPrime(1);

            //Assert
            Assert.False(result, "One Should not be Prime");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public void IsPrime_InputLessThanTwo_ReturnFalse(int input)
        {
            // Arrange 
            // var primeService = new PrimeService();
            // Act
            var result = _primeService.IsPrime(input);
            // Assert
            Assert.False(result, $"{input} Should not be Prime");
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(7)]
        public void IsPrime_PrimeLessThan10_ReturnTrue(int input)
        {
            var result = _primeService.IsPrime(input);
            Assert.True(result, $"{input} should be Prime");
        }

        [Theory]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(8)]
        [InlineData(9)]
        public void IsPrime_NonePrimeLessThan10_ReturnFalse(int input)
        {
            var result = _primeService.IsPrime(input);
            Assert.False(result, $"{input} should not be Prime");
        }
    }
}
