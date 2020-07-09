using Microsoft.AspNetCore.Server.IISIntegration;
using System;

namespace IdentityServer.Quickstart.Account
{
    public class AccountOptions
    {
        public static bool AllowLocalLogin = true;

        public static bool AllowRememberLogin = true;

        public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);

        public static bool ShowLogoutPrompt = true;

        public static bool AutomaticRedirectAfterSignOut = false;

        // Specify the Windows authentication scheme being used
        public static readonly string WindowsAuthenticationSchemeName = IISDefaults.AuthenticationScheme;

        public static bool IncludeWindowsGroups = false;

        public static string InvalidCredentialsErrorMessage = "Invalid username or password";
    }
}