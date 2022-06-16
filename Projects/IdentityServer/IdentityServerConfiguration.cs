using Duende.IdentityServer.Models;
using System.Collections.Generic;

namespace IdentityServer;

public static class IdentityServerConfiguration
{
    public static IEnumerable<ApiScope> Scopes => new[]
    {
        new ApiScope("employee:read", "The ability to read employee data."),
        new ApiScope("employee:create", "The ability create employee data."),
        new ApiScope("employee:edit", "The ability to edit employee data."),
        new ApiScope("employee:delete", "The ability to delete employee data.")
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
                "employee:read",
                "employee:create",
                "employee:edit",
                "employee:delete"
            }
        }
    };
}