using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public static class IdentityServerConfiguration
    {
        public static IEnumerable<ApiResource> Resources => new[]
        {
            new ApiResource("employee:read", "The ability to read employee data."),
            new ApiResource("employee:create", "The ability create employee data."),
            new ApiResource("employee:edit", "The ability to edit employee data."),
            new ApiResource("employee:delete", "The ability to delete employee data.")
        };

        public static IEnumerable<Client> Clients => new[]
        {
            new Client
            {
                ClientId          = "testing_client_identifier",
                ClientName        = "A testing client credentials client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets     = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
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
}