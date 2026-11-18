using AccessTokenClient.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;

namespace AccessTokenClient.Extensions;

/// <summary>
/// This static class contains an extension method used to add the
/// <see cref="ITokenClient"/> and dependencies to the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the token client to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="builderAction">
    /// An optional action used to configure the <see cref="IHttpClientBuilder"/> instance that is returned
    /// when the <see cref="ITokenClient"/> implementation is registered in the service collection via the
    /// AddHttpClient extension method. This can be used to register an <see cref="HttpMessageHandler"/> for
    /// the token client, or to configure a retry policy for the <see cref="ITokenClient"/> to handle transient
    /// errors that may be encountered. 
    /// </param>
    /// <returns>The service collection instance.</returns>
    public static IServiceCollection AddAccessTokenClient(this IServiceCollection services, Action<IHttpClientBuilder>? builderAction = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var httpClientBuilder = services.AddHttpClient<ITokenClient, TokenClient>("AccessTokenClient.TokenClient");

        builderAction?.Invoke(httpClientBuilder);

        return services;
    }

    /// <summary>
    /// Enables caching for the token client.
    /// </summary>
    /// <typeparam name="T">
    /// An implementation of the <see cref="ITokenResponseCache"/> interface to register.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="action">An optional action used to configure the token client cache options.</param>
    /// <returns>The service collection instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the token client has not been registered using the <see cref="AddAccessTokenClient"/> method.
    /// </exception>
    public static IServiceCollection AddAccessTokenClientCache<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IServiceCollection services, Action<TokenClientCacheOptions>? action = null) where T : ITokenResponseCache
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new TokenClientCacheOptions();
        action?.Invoke(options);

        if (options.ExpirationBuffer <= 0)
        {
            throw new ArgumentException("The expiration buffer must be greater than 0.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(options.CacheKeyPrefix);

        // Caching has already been enabled, so do not decorate the token client a second time:
        if (services.Any(descriptor => descriptor.ServiceType == typeof(TokenClientCacheOptions)))
        {
            return services;
        }

        var tokenClientDescriptor = services.LastOrDefault(descriptor => descriptor.ServiceType == typeof(ITokenClient) && !descriptor.IsKeyedService);

        if (tokenClientDescriptor == null)
        {
            throw new InvalidOperationException($"The token client has not been registered. Call {nameof(AddAccessTokenClient)} before calling {nameof(AddAccessTokenClientCache)}.");
        }

        services.TryAddSingleton(options);
        services.TryAddSingleton<IKeyGenerator, TokenRequestKeyGenerator>();
        services.TryAddSingleton(typeof(ITokenResponseCache), typeof(T));
        services.TryAddSingleton<IAccessTokenTransformer, DefaultAccessTokenTransformer>();

        DecorateTokenClient(services, tokenClientDescriptor);

        return services;
    }

    /// <summary>
    /// Replaces the token client registration with one that wraps the original token client in the
    /// <see cref="TokenClientCachingDecorator"/>. The decorator is created directly instead of through
    /// reflection so its constructor is not removed when the application is trimmed or AOT compiled.
    /// </summary>
    private static void DecorateTokenClient(IServiceCollection services, ServiceDescriptor descriptor)
    {
        var createTokenClient = GetTokenClientFactory(descriptor);

        var decoratedDescriptor = ServiceDescriptor.Describe(typeof(ITokenClient), provider => new TokenClientCachingDecorator(
            provider.GetRequiredService<ILogger<TokenClientCachingDecorator>>(),
            createTokenClient(provider),
            provider.GetRequiredService<TokenClientCacheOptions>(),
            provider.GetRequiredService<ITokenResponseCache>(),
            provider.GetRequiredService<IKeyGenerator>(),
            provider.GetRequiredService<IAccessTokenTransformer>()
        ), descriptor.Lifetime);

        services[services.IndexOf(descriptor)] = decoratedDescriptor;
    }

    private static Func<IServiceProvider, ITokenClient> GetTokenClientFactory(ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationFactory != null)
        {
            var factory = descriptor.ImplementationFactory;
            return provider => (ITokenClient)factory(provider);
        }

        if (descriptor.ImplementationInstance != null)
        {
            var instance = (ITokenClient)descriptor.ImplementationInstance;
            return _ => instance;
        }

        var implementationType = descriptor.ImplementationType!;
        return provider => (ITokenClient)ActivatorUtilities.CreateInstance(provider, implementationType);
    }
}
