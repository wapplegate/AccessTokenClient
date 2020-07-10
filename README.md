[![Build Status](https://dev.azure.com/bill-applegate/AccessTokenClient/_apis/build/status/wapplegate.AccessTokenClient?branchName=development)](https://dev.azure.com/bill-applegate/AccessTokenClient/_build/latest?definitionId=4&branchName=development)

![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/bill-applegate/AccessTokenClient/4)

## About

This library provides an access token client that can be used when you need to make `client_credentials` OAuth requests. Instead of re-writing the same access token clients over and over again in your services, you can use this package to make those token requests easily. The package also includes the ability to cache access tokens in memory so they can be reused.

## Installation

In order to begin using the token client, you first need to install two packages `AccessTokenClient` and `AccessTokenClient.Extensions`. The extensions package includes a service collection extension method which will create the required service registrations for you.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    services.AddMemoryCache();

    services.AddAccessTokenClient();
}
```

By default, the `TokenClient` uses a memory cache to store tokens after they have been retrieved. You'll need to register an implementation of `IMemoryCache` using the `AddMemoryCache` service collection extension method or override the `ITokenResponseCache` interface used by the `TokenClient` to provide your own caching implementation.

You can also configure the access token client options via the `AddAccessTokenClient` method. You can specify whether to enable or disable caching as seen below:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    services.AddAccessTokenClient(options => options.EnableCaching = false);
}
```

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
    TokenEndpoint    = "http://www.oauth-endpoint.com",
    ClientIdentifier = "identifier",
    ClientSecret     = "secret",
    Scopes           = new[] { "scope:read" }
});
```

The `ITokenClient` interface has one method `RequestAccessToken`. This method will request an access token using the `client_credentials` grant from the specified endpoint using the supplied identifier, secret, and scopes. If the request is successful, a `TokenResponse` will be returned which contains the access token, token type, and expiration fields.
