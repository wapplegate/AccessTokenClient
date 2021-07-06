using IdentityServer.Quickstart.Account;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IdentityServer.Quickstart
{
    public static class Extensions
    {
        /// <summary>
        /// Determines whether the client is configured to use PKCE.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="clientIdentifier">The client identifier.</param>
        /// <returns>A value indicating whether the client requires PKCE.</returns>
        public static async Task<bool> IsPkceClientAsync(this IClientStore store, string clientIdentifier)
        {
            if (string.IsNullOrWhiteSpace(clientIdentifier))
            {
                return false;
            }

            var client = await store.FindEnabledClientByIdAsync(clientIdentifier);
            return client?.RequirePkce == true;
        }

        public static IActionResult LoadingPage(this Controller controller, string viewName, string redirectUri)
        {
            controller.HttpContext.Response.StatusCode = 200;
            controller.HttpContext.Response.Headers["Location"] = "";
            
            return controller.View(viewName, new RedirectViewModel { RedirectUrl = redirectUri });
        }
    }
}
