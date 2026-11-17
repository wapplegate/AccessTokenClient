# AccessTokenClient.Extensions

Service collection extension methods for registering the [AccessTokenClient](https://www.nuget.org/packages/AccessTokenClient) token client, along with a default retry policy for token requests.

## Installation

```shell
dotnet add package AccessTokenClient.Extensions
```

## Registering the Token Client

```csharp
services.AddAccessTokenClient();
```

This registers `ITokenClient`, which can then be injected into controllers and services to make client credentials token requests.

## Retrying Failed Token Requests

`AddAccessTokenClient` accepts an action that configures the `IHttpClientBuilder` used to register the token client. Use it to add a retry policy for requests to the token endpoint. A default policy is included:

```csharp
services.AddAccessTokenClient(builder =>
{
    builder.AddPolicyHandler((provider, _) =>
    {
        var logger = provider.GetService<ILogger<ITokenClient>>();
        return AccessTokenClientPolicy.GetDefaultRetryPolicy(logger);
    });
});
```

The default policy retries twice, waiting one second between attempts, when a transient error (5XX or 408) or a 404 is encountered. Retries are not enabled unless a policy is added.

## Caching

Access tokens are not cached by default. Use `AddAccessTokenClientCache` to enable caching:

```csharp
services.AddMemoryCache();

services.AddAccessTokenClient().AddAccessTokenClientCache<MemoryTokenResponseCache>(options =>
{
    options.ExpirationBuffer = 5;
    options.CacheKeyPrefix   = "AccessTokenClient";
});
```

`ExpirationBuffer` is the number of minutes subtracted from the token's lifetime when it is cached, so a token is never cached longer than it is valid. `CacheKeyPrefix` is the prefix used for generated cache keys. The values shown are the defaults.

The type argument must implement `ITokenResponseCache`, so a custom cache such as a distributed cache can be used in place of `MemoryTokenResponseCache`.

## Documentation

Full documentation is available in the [GitHub repository](https://github.com/wapplegate/AccessTokenClient).
