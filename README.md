## About

This library provides a token client that can be used when you need to make `client_credentials` OAuth requests. Instead of re-writing the same token clients over and over again in your services, you can use this package to make those token requests easily. The package also includes advanced features like encryption and caching so access tokens can be reused.

## Installation

In order to begin using the token client, you first need to install two packages `AccessTokenClient` and `AccessTokenClient.Extensions`. The extensions package includes a service collection extension method which will create the required service registrations for you.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    services.AddTokenClient();
}
```

You can also configure the token client options via the `AddTokenClient` method. You can specify whether to enable or disable caching as seen below:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    services.AddTokenClient(options => options.EnableCaching = false);
}
```

## Usage

Once configured, you can then inject an instance of the `IAccessTokenClient` type into your controllers or services.

```csharp
public class Service
{
    private readonly IAccessTokenClient client;

    public Service(IAccessTokenClient client)
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

The `IAccessTokenClient` interface has one method `RequestAccessToken`. This method will make a `client_credentials` OAuth request to the specified endpoint using the supplied client identifier, client secret, and scopes. If the request is successful, a `TokenResponse` will be returned which contains the access token, token type, and expiration fields.
