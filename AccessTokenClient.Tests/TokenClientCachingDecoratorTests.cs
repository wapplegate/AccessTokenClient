using AccessTokenClient.Caching;
using AccessTokenClient.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using System.Net;
using Xunit;

namespace AccessTokenClient.Tests;

public class TokenClientCachingDecoratorTests
{
    [Fact]
    public async Task EnsureTokenResponseIsReturnedWhenCacheKeyIsFound()
    {
        const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

        var logger = new NullLogger<TokenClient>();
        var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
        var httpClient = new HttpClient(messageHandler);

        // Set up the token response cache mock:
        var cacheMock = new Mock<ITokenResponseCache>();
        cacheMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new TokenResponse
        {
            AccessToken = "1234567890"
        });

        var decoratorLogger = new NullLogger<TokenClientCachingDecorator>();

        // Set up the key generator mock:
        var keyGeneratorMock = new Mock<IKeyGenerator>();
        keyGeneratorMock.Setup(m => m.GenerateTokenRequestKey(It.IsAny<TokenRequest>(), It.IsAny<string>())).Returns("KEY-123");

        var mockTransformer = new Mock<IAccessTokenTransformer>();

        // Set up the access token client, token client, and the caching decorator:
        var tokenClient = new TokenClient(logger, httpClient);

        ITokenClient cachingDecorator = new TokenClientCachingDecorator(
            decoratorLogger,
            tokenClient,
            new TokenClientCacheOptions(),
            cacheMock.Object,
            keyGeneratorMock.Object,
            mockTransformer.Object
        );

        var tokenResponse = await cachingDecorator.RequestAccessToken(new TokenRequest
        {
            TokenEndpoint    = "http://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope:read"]
        });

        tokenResponse.ShouldNotBeNull();

        messageHandler.NumberOfCalls.ShouldBe(0);
    }

    [Fact]
    public async Task EnsureTokenResponseIsReturnedWhenCacheKeyIsNotFound()
    {
        const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

        var logger = new NullLogger<TokenClient>();
        var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
        var httpClient = new HttpClient(messageHandler);

        var decoratorLogger = new NullLogger<TokenClientCachingDecorator>();

        // Set up the token response cache mock:
        var cacheMock = new Mock<ITokenResponseCache>();
        cacheMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((TokenResponse)null);

        // Set up the key generator mock:
        var keyGeneratorMock = new Mock<IKeyGenerator>();
        keyGeneratorMock.Setup(m => m.GenerateTokenRequestKey(It.IsAny<TokenRequest>(), It.IsAny<string>())).Returns("KEY-123");

        var mockTransformer = new Mock<IAccessTokenTransformer>();

        // Set up the token client and the caching decorator:
        var tokenClient = new TokenClient(logger, httpClient);

        ITokenClient cachingDecorator = new TokenClientCachingDecorator(
            decoratorLogger,
            tokenClient,
            new TokenClientCacheOptions(),
            cacheMock.Object,
            keyGeneratorMock.Object,
            mockTransformer.Object
        );

        var tokenResponse = await cachingDecorator.RequestAccessToken(new TokenRequest
        {
            TokenEndpoint    = "http://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope:read"]
        });

        tokenResponse.ShouldNotBeNull();
        tokenResponse.AccessToken.ShouldNotBeNull();
        tokenResponse.AccessToken.ShouldBe("1234567890");

        messageHandler.NumberOfCalls.ShouldBe(1);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EnsureTokenResponseReturnedWhenCacheSetOperationIsSuccessfulOrFails(bool setResult)
    {
        const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

        var logger = new NullLogger<TokenClient>();
        var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
        var httpClient = new HttpClient(messageHandler);

        var decoratorLogger = new NullLogger<TokenClientCachingDecorator>();

        // Set up the token response cache mock:
        var cacheMock = new Mock<ITokenResponseCache>();

        cacheMock
            .Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenResponse?)null);

        cacheMock
            .Setup(m => m.Set(It.IsAny<string>(),It.IsAny<TokenResponse>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(setResult);

        // Set up the key generator mock:
        var keyGeneratorMock = new Mock<IKeyGenerator>();
        keyGeneratorMock.Setup(m => m.GenerateTokenRequestKey(It.IsAny<TokenRequest>(), It.IsAny<string>())).Returns("KEY-123");

        var mockTransformer = new Mock<IAccessTokenTransformer>();

        // Set up the token client and the caching decorator:
        var tokenClient = new TokenClient(logger, httpClient);

        ITokenClient cachingDecorator = new TokenClientCachingDecorator(
            decoratorLogger,
            tokenClient,
            new TokenClientCacheOptions(),
            cacheMock.Object,
            keyGeneratorMock.Object,
            mockTransformer.Object
        );

        var tokenResponse = await cachingDecorator.RequestAccessToken(new TokenRequest
        {
            TokenEndpoint    = "http://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope:read"]
        });

        tokenResponse.ShouldNotBeNull();
        tokenResponse.AccessToken.ShouldNotBeNull();
        tokenResponse.AccessToken.ShouldBe("1234567890");

        messageHandler.NumberOfCalls.ShouldBe(1);
    }

    [Fact]
    public async Task EnsureTokenResponseIsReturnedWhenScopesAreNotSpecified()
    {
        const string Response = @"{""access_token"":""1234567890"",""token_type"":""Bearer"",""expires_in"":7199}";

        var logger = new NullLogger<TokenClient>();
        var messageHandler = new MockHttpMessageHandler(Response, HttpStatusCode.OK);
        var httpClient = new HttpClient(messageHandler);

        var decoratorLogger = new NullLogger<TokenClientCachingDecorator>();

        // Set up the token response cache mock:
        var cacheMock = new Mock<ITokenResponseCache>();
        cacheMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((TokenResponse)null);

        // Set up the key generator mock:
        var keyGeneratorMock = new Mock<IKeyGenerator>();
        keyGeneratorMock.Setup(m => m.GenerateTokenRequestKey(It.IsAny<TokenRequest>(), It.IsAny<string>())).Returns("KEY-123");

        var mockTransformer = new Mock<IAccessTokenTransformer>();

        // Set up the token client and the caching decorator:
        var tokenClient = new TokenClient(logger, httpClient);

        var cachingDecorator = new TokenClientCachingDecorator(
            decoratorLogger,
            tokenClient,
            new TokenClientCacheOptions(),
            cacheMock.Object,
            keyGeneratorMock.Object,
            mockTransformer.Object
        );

        var tokenResponse = await cachingDecorator.RequestAccessToken(new TokenRequest
        {
            TokenEndpoint    = "http://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
        });

        tokenResponse.ShouldNotBeNull();
        tokenResponse.AccessToken.ShouldNotBeNull();
        tokenResponse.AccessToken.ShouldBe("1234567890");

        messageHandler.NumberOfCalls.ShouldBe(1);
    }

    [Fact]
    public void EnsureExceptionThrownWhenCancellationRequested()
    {
        Func<Task<TokenResponse>> creationAction = async () =>
        {
            // Cancel the token before executing the decorator so it throws immediately:
            var source = new CancellationTokenSource();
            source.Cancel();

            var decorator = new TokenClientCachingDecorator(
                new NullLogger<TokenClientCachingDecorator>(),
                new TokenClient(new NullLogger<TokenClient>(), new HttpClient()),
                new TokenClientCacheOptions(),
                new MemoryTokenResponseCache(new MemoryCache(new MemoryCacheOptions())),
                new TokenRequestKeyGenerator(),
                new DefaultAccessTokenTransformer());

            var response = await decorator.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "http://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
                Scopes           = ["scope:read"]
            }, cancellationToken: source.Token);

            return response;
        };

        creationAction.ShouldThrowAsync<OperationCanceledException>();
    }
}