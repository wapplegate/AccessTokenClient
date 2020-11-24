using System;
using AccessTokenClient.Expiration;
using FluentAssertions;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class ExpirationCalculatorTests
    {
        [Fact]
        public void EnsureFiveMinutesRemovedFromTokenLifetime()
        {
            var calculator = new DefaultExpirationCalculator();

            var result = calculator.CalculateExpiration(new TokenResponse
            {
                AccessToken = "123",
                ExpiresIn   = 5000,
                TokenType   = "token-type"
            });

            result.Should().Be(TimeSpan.FromMinutes(5000 / 60 - 5));
        }
    }
}