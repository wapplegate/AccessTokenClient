# AccessTokenClient

A token client for making OAuth client credentials requests. Instead of re-writing the same access token client for every service, use this package to request tokens, cache them so they can be reused, and automatically attach them to outgoing HTTP requests.

## Installation

```shell
dotnet add package AccessTokenClient
dotnet add package AccessTokenClient.Extensions
```

The [AccessTokenClient.Extensions](https://www.nuget.org/packages/AccessTokenClient.Extensions) package contains the service collection extension methods used to register the token client:

```csharp
services.AddAccessTokenClient();
```

## Requesting an Access Token

Inject `ITokenClient` and call `RequestAccessToken`:

```csharp
public class Service(ITokenClient client)
{
    public async Task<string?> GetAccessToken()
    {
        var tokenResponse = await client.RequestAccessToken(new TokenRequest
        {
            TokenEndpoint    = "https://www.token-endpoint.com/token",
            ClientIdentifier = "client-identifier",
            ClientSecret     = "client-secret",
            Scopes           = ["scope:read"]
        });

        return tokenResponse.AccessToken;
    }
}
```

The request uses the client credentials grant. If it succeeds, the returned `TokenResponse` contains the access token and its expiration in seconds.

## Caching

Access tokens are not cached by default. To cache them in memory, register `MemoryTokenResponseCache`:

```csharp
services.AddMemoryCache();

services.AddAccessTokenClient().AddAccessTokenClientCache<MemoryTokenResponseCache>();
```

To use a different store, such as a distributed cache, implement `ITokenResponseCache` and pass that type instead.

## Automatically Adding the Access Token to Outgoing Requests

Implement `ITokenRequestOptions`, register it in the service collection, and add the access token handler to a client registered with `AddHttpClient`:

```csharp
services.AddHttpClient<TestClient>().AddClientAccessTokenHandler<TestClientTokenOptions>();
```

The handler requests an access token and adds it to the authorization header of every request made by `TestClient`.

## Documentation

Full documentation is available in the [GitHub repository](https://github.com/wapplegate/AccessTokenClient).
