using Duende.IdentityServer.Models;
using System.Collections.Generic;

namespace IdentityServer;

public static class IdentityServerConfiguration
{
    public static IEnumerable<ApiScope> Scopes => new[]
    {
        new ApiScope("movie:read", "The ability to read movie data."),
        new ApiScope("movie:create", "The ability create a movie."),
        new ApiScope("movie:edit", "The ability to edit movie data."),
        new ApiScope("movie:delete", "The ability to delete a movie.")
    };

    public static IEnumerable<Client> Clients => new[]
    {
        new Client
        {
            ClientId          = "testing_client_identifier",
            ClientName        = "A testing client credentials client",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets     =
            {
                new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256())
            },
            AllowedScopes     =
            {
                "movie:read",
                "movie:create",
                "movie:edit",
                "movie:delete"
            }
        }
    };
}