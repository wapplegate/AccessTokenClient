﻿using FluentAssertions;
using Moq;
using System.Net;
using Xunit;

namespace AccessTokenClient.Tests;

public class DelegatingHandlerTests
{
    [Fact]
    public void EnsureExceptionThrownWhenOptionsAreNull()
    {
        ITokenRequestOptions options = new Options();
        Action action = () => _ = new AccessTokenDelegatingHandler(options, null);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureExceptionThrownWhenTokenClientIsNull()
    {
        var mockClient = new Mock<ITokenClient>();
        Action action = () => _ = new AccessTokenDelegatingHandler(null, mockClient.Object);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task EnsureAccessTokenAddedToRequest()
    {
        ITokenRequestOptions options = new Options();
        var mockClient = new Mock<ITokenClient>();

        mockClient
            .Setup(m => m.RequestAccessToken(It.IsAny<TokenRequest>(), It.IsAny<Func<TokenRequest, Task<TokenResponse>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenResponse
            {
                AccessToken = "1234567890",
                ExpiresIn   = 3000
            });

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var handler = new AccessTokenDelegatingHandler(options, mockClient.Object)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(handler);
        await invoker.SendAsync(httpRequestMessage, new CancellationToken());
    }
}

public class TestHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.OK), cancellationToken);
    }
}

public class Options : ITokenRequestOptions
{
    public string TokenEndpoint { get; set; }

    public string ClientIdentifier { get; set; }

    public string ClientSecret { get; set; }

    public string[] Scopes { get; set; }
}