using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using Microsoft.Identity.Client;

namespace ClientWrapper
{
    public class Application
    {
        private static string ClientId = "[Enter Client Id]";
        private static string Authority = "[Enter Authority]";
        private static string[] Scopes = { "[Enter scope]" };

        [UnmanagedCallersOnly(EntryPoint = "GetAccessToken")]
        public static IntPtr GetAccessToken()
        {
            var application = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(new Uri(Authority))
                .WithClientCapabilities(new [] { "cp1" })
                .WithDefaultRedirectUri()
                .Build();

            var accounts = application.GetAccountsAsync().GetAwaiter().GetResult().ToList();
            
            var result = application.AcquireTokenInteractive(Scopes)
                .WithAccount(accounts.FirstOrDefault())
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync()
                .GetAwaiter().GetResult();
                
            var accessToken = Marshal.StringToHGlobalAnsi(result.AccessToken);
            return accessToken;
        }
    }
}
