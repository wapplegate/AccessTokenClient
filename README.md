[![Build Status](https://dev.azure.com/bill-applegate/AccessTokenClient/_apis/build/status/wapplegate.AccessTokenClient?branchName=development)](https://dev.azure.com/bill-applegate/AccessTokenClient/_build/latest?definitionId=4&branchName=development)

![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/bill-applegate/AccessTokenClient/4)

## About

This library provides an access token client that can be used when you need to make client credentials OAuth requests. Instead of re-writing the same access token clients over and over again in your services, you can use this package to make those token requests. The package also includes the ability to cache access tokens in memory so they can be reused and provides extension points so you can create a cache implementation that suits your needs.

## Installation

In order to begin using the token client, install the `AccessTokenClient` and `AccessTokenClient.Extensions` nuget packages. The extensions package includes a service collection extension method which will create the required service registrations for you.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAccessTokenClient();
}
```

## Caching

By default, access tokens will not be cached. To enable caching, use the `AddAccessTokenClientCache` extension method and specify `MemoryTokenResponseCache` as a generic type argument as shown below.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMemoryCache();

    services.AddAccessTokenClient().AddAccessTokenClientCache<MemoryTokenResponseCache>();
}
```

You'll need to register an implementation of `IMemoryCache` using the `AddMemoryCache` service collection extension method provided by Microsoft in order to use the `MemoryTokenResponseCache`. The `AddAccessTokenClientCache` method accepts a generic type which must implement the `ITokenResponseCache` interface. A custom token response cache can implement this interface and be used instead of the default. A common usage scenario might include using a distributed cache for access token instead of a memory cache.

## Usage

Once configured, you can then inject an instance of the `ITokenClient` type into your controllers or services.

```csharp
public class Service
{
    private readonly ITokenClient client;

    public Service(ITokenClient client)
    {
        this.client = client;
    }
}
```

Here is an example of using the token client to make a request to your token endpoint:

```csharp
var tokenResponse = await client.RequestAccessToken(new TokenRequest
{
    TokenEndpoint    = "http://www.token-endpoint.com/token",
    ClientIdentifier = "client-identifier",
    ClientSecret     = "client-secret",
    Scopes           = new[] { "scope:read" }
});
```

The `ITokenClient` interface has one method `RequestAccessToken`. This method will request an access token using the client credentials grant from the specified endpoint using the supplied client identifier, secret, and scopes. If the request is successful, a `TokenResponse` will be returned which contains the access token and expiration fields.
