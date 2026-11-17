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
            TokenEndpoint    = "https://www.token-endpoint.com",
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
        cacheMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((TokenResponse)null!);

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
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope:read"]
        });

        tokenResponse.ShouldNotBeNull();
        tokenResponse.AccessToken!.ShouldNotBeNull();
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
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope:read"]
        });

        tokenResponse.ShouldNotBeNull();
        tokenResponse.AccessToken!.ShouldNotBeNull();
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
        cacheMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((TokenResponse)null!);

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
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
        });

        tokenResponse.ShouldNotBeNull();
        tokenResponse.AccessToken!.ShouldNotBeNull();
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
            await source.CancelAsync();

            var decorator = new TokenClientCachingDecorator(
                new NullLogger<TokenClientCachingDecorator>(),
                new TokenClient(new NullLogger<TokenClient>(), new HttpClient()),
                new TokenClientCacheOptions(),
                new MemoryTokenResponseCache(new MemoryCache(new MemoryCacheOptions())),
                new TokenRequestKeyGenerator(),
                new DefaultAccessTokenTransformer());

            var response = await decorator.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "https://www.token-endpoint.com",
                ClientIdentifier = "client-identifier",
                ClientSecret     = "client-secret",
                Scopes           = ["scope:read"]
            }, cancellationToken: source.Token);

            return response;
        };

        creationAction.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EnsureExceptionThrownWhenTokenRequestIsNull()
    {
        var decorator = CreateDecorator(new StubTokenClient(expiresIn: 3600), new DefaultAccessTokenTransformer(), CreateMemoryCache());

        Func<Task<TokenResponse>> function = async () => await decorator.RequestAccessToken(null!);

        await function.ShouldThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(60)]
    [InlineData(299)]
    [InlineData(300)]
    public async Task EnsureTokenResponseIsReturnedAndNotCachedWhenTokenLifetimeIsShorterThanExpirationBuffer(int expiresIn)
    {
        var tokenClient = new StubTokenClient(expiresIn);

        var cacheMock = new Mock<ITokenResponseCache>();
        cacheMock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((TokenResponse?)null);

        var decorator = CreateDecorator(tokenClient, new DefaultAccessTokenTransformer(), cacheMock.Object);

        var tokenResponse = await decorator.RequestAccessToken(CreateTokenRequest());

        tokenResponse.AccessToken.ShouldBe("1234567890");
        tokenResponse.ExpiresIn.ShouldBe(expiresIn);

        tokenClient.NumberOfCalls.ShouldBe(1);

        cacheMock.Verify(m => m.Set(It.IsAny<string>(), It.IsAny<TokenResponse>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnsureTokenResponseIsReturnedWhenTokenLifetimeIsShorterThanExpirationBufferUsingMemoryCache()
    {
        var tokenClient = new StubTokenClient(expiresIn: 300);

        var decorator = CreateDecorator(tokenClient, new DefaultAccessTokenTransformer(), CreateMemoryCache());

        var first  = await decorator.RequestAccessToken(CreateTokenRequest());
        var second = await decorator.RequestAccessToken(CreateTokenRequest());

        first.AccessToken.ShouldBe("1234567890");
        second.AccessToken.ShouldBe("1234567890");

        tokenClient.NumberOfCalls.ShouldBe(2);
    }

    [Fact]
    public async Task EnsureTokenResponseIsCachedForTheTokenLifetimeMinusTheExpirationBuffer()
    {
        var cacheMock = new Mock<ITokenResponseCache>();

        cacheMock
            .Setup(m => m.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenResponse?)null);

        cacheMock
            .Setup(m => m.Set(It.IsAny<string>(), It.IsAny<TokenResponse>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var decorator = CreateDecorator(new StubTokenClient(expiresIn: 7199), new DefaultAccessTokenTransformer(), cacheMock.Object);

        await decorator.RequestAccessToken(CreateTokenRequest());

        // 7199 seconds minus the default five minute buffer:
        cacheMock.Verify(m => m.Set(It.IsAny<string>(), It.IsAny<TokenResponse>(), TimeSpan.FromSeconds(6899), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnsureOriginalAccessTokenIsReturnedWhenTransformerIsUsedWithMemoryCache()
    {
        var tokenClient = new StubTokenClient(expiresIn: 3600);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var decorator = CreateDecorator(tokenClient, new ReversingAccessTokenTransformer(), new MemoryTokenResponseCache(memoryCache));

        var request = CreateTokenRequest();

        var first  = await decorator.RequestAccessToken(request);
        var second = await decorator.RequestAccessToken(request);
        var third  = await decorator.RequestAccessToken(request);

        first.AccessToken.ShouldBe("1234567890");
        second.AccessToken.ShouldBe("1234567890");
        third.AccessToken.ShouldBe("1234567890");

        tokenClient.NumberOfCalls.ShouldBe(1);

        // The converted access token is the value that should be held by the cache:
        var key = new TokenRequestKeyGenerator().GenerateTokenRequestKey(request, new TokenClientCacheOptions().CacheKeyPrefix);

        memoryCache.Get<TokenResponse>(key)!.AccessToken.ShouldBe("0987654321");
    }

    [Fact]
    public async Task EnsureCachedTokenResponseIsNotModifiedWhenReturnedTokenResponseIsChanged()
    {
        var decorator = CreateDecorator(new StubTokenClient(expiresIn: 3600), new DefaultAccessTokenTransformer(), CreateMemoryCache());

        var request = CreateTokenRequest();

        var first = await decorator.RequestAccessToken(request);

        first.AccessToken = "modified";

        var second = await decorator.RequestAccessToken(request);

        second.ShouldNotBeSameAs(first);
        second.AccessToken.ShouldBe("1234567890");

        second.AccessToken = "modified";

        var third = await decorator.RequestAccessToken(request);

        third.AccessToken.ShouldBe("1234567890");
    }

    [Fact]
    public async Task EnsureTokenEndpointIsOnlyCalledOnceWhenConcurrentRequestsAreMade()
    {
        var tokenClient = new StubTokenClient(expiresIn: 3600, delay: TimeSpan.FromMilliseconds(100));

        var cache = CreateMemoryCache();

        // The token client is transient, so use a separate decorator instance for each request:
        var tasks = Enumerable.Range(0, 25).Select(_ => Task.Run(() =>
        {
            var decorator = CreateDecorator(tokenClient, new DefaultAccessTokenTransformer(), cache);

            return decorator.RequestAccessToken(CreateTokenRequest());
        }));

        var tokenResponses = await Task.WhenAll(tasks);

        tokenResponses.ShouldAllBe(response => response.AccessToken == "1234567890");

        tokenClient.NumberOfCalls.ShouldBe(1);
    }

    [Fact]
    public async Task EnsureTokenRequestCanBeRetriedWhenPreviousRequestFails()
    {
        var tokenClientMock = new Mock<ITokenClient>();

        tokenClientMock
            .SetupSequence(m => m.RequestAccessToken(It.IsAny<TokenRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("The token request failed."))
            .ReturnsAsync(new TokenResponse { AccessToken = "1234567890", ExpiresIn = 3600 });

        var decorator = CreateDecorator(tokenClientMock.Object, new DefaultAccessTokenTransformer(), CreateMemoryCache());

        var request = CreateTokenRequest();

        Func<Task<TokenResponse>> function = async () => await decorator.RequestAccessToken(request);

        await function.ShouldThrowAsync<HttpRequestException>();

        var tokenResponse = await decorator.RequestAccessToken(request).WaitAsync(TimeSpan.FromSeconds(10));

        tokenResponse.AccessToken.ShouldBe("1234567890");
    }

    private static TokenClientCachingDecorator CreateDecorator(ITokenClient tokenClient, IAccessTokenTransformer transformer, ITokenResponseCache cache)
    {
        return new TokenClientCachingDecorator(
            new NullLogger<TokenClientCachingDecorator>(),
            tokenClient,
            new TokenClientCacheOptions(),
            cache,
            new TokenRequestKeyGenerator(),
            transformer
        );
    }

    private static MemoryTokenResponseCache CreateMemoryCache()
    {
        return new MemoryTokenResponseCache(new MemoryCache(new MemoryCacheOptions()));
    }

    private static TokenRequest CreateTokenRequest()
    {
        return new TokenRequest
        {
            TokenEndpoint    = "https://www.token-endpoint.com",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope:read"]
        };
    }

    private sealed class StubTokenClient : ITokenClient
    {
        private readonly int expiresIn;

        private readonly TimeSpan delay;

        private int numberOfCalls;

        public StubTokenClient(int expiresIn, TimeSpan delay = default)
        {
            this.expiresIn = expiresIn;
            this.delay     = delay;
        }

        public int NumberOfCalls => numberOfCalls;

        public async Task<TokenResponse> RequestAccessToken(TokenRequest request, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref numberOfCalls);

            await Task.Delay(delay, cancellationToken);

            return new TokenResponse
            {
                AccessToken = "1234567890",
                ExpiresIn   = expiresIn
            };
        }
    }

    private sealed class ReversingAccessTokenTransformer : IAccessTokenTransformer
    {
        public string Convert(string accessToken)
        {
            return new string(accessToken.Reverse().ToArray());
        }

        public string Revert(string value)
        {
            return new string(value.Reverse().ToArray());
        }
    }
}
