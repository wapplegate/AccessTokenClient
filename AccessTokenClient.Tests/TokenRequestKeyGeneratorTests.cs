﻿using AccessTokenClient.Keys;
using AccessTokenClient.Tests.Helpers;
using FluentAssertions;
using System;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class TokenRequestKeyGeneratorTests
    {
        [Fact]
        public void EnsureKeyGeneratedSuccessfully()
        {
            var generator = new TokenRequestKeyGenerator();

            var request = new TokenRequest
            {
                ClientIdentifier = "ClientIdentifier",
                ClientSecret     = "ClientSecret",
                Scopes           = new[] { "scope_1", "scope_2", "scope_3" },
                TokenEndpoint    = "https://www.token-endpoint.com"
            };

            var key = generator.GenerateTokenRequestKey(request);

            key.ShouldNotBeNull();
        }

        [Fact]
        public void EnsureExceptionThrownWhenRequestIsInvalid()
        {
            var generator = new TokenRequestKeyGenerator();

            var request = new TokenRequest
            {
                ClientIdentifier = "",
                ClientSecret     = "ClientSecret",
                Scopes           = new[] { "scope_1", "scope_2", "scope_3" },
                TokenEndpoint    = "https://www.token-endpoint.com"
            };

            Action action = () => generator.GenerateTokenRequestKey(request);

            action.Should().Throw<Exception>();
        }

        [Fact]
        public void EnsureRequestGeneratesDifferentKeysWhenCaseIsDifferent()
        {
            var generator = new TokenRequestKeyGenerator();

            var tokenRequestOne = new TokenRequest
            {
                ClientIdentifier = "ClientIdentifier",
                ClientSecret     = "ClientSecret",
                Scopes           = new[] { "scope_1", "scope_2", "scope_3" },
                TokenEndpoint    = "https://www.token-endpoint.com"
            };

            var keyOne = generator.GenerateTokenRequestKey(tokenRequestOne);

            var tokenRequestTwo = new TokenRequest
            {
                ClientIdentifier = "CLIENTIDENTIFIER",
                ClientSecret     = "CLIENTSECRET",
                Scopes           = new[] { "SCOPE_1", "SCOPE_2", "SCOPE_3" },
                TokenEndpoint    = "https://www.token-endpoint.com"
            };

            var keyTwo = generator.GenerateTokenRequestKey(tokenRequestTwo);

            keyOne.ShouldNotBeNull();
            keyTwo.ShouldNotBeNull();

            keyOne.Should().NotBeEquivalentTo(keyTwo);
        }
    }
}