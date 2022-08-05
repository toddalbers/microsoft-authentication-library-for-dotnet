using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp12
{
    internal class Program
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        static async Task Main(string[] args)
        {
            IntPtr _parentHandle = GetForegroundWindow();
            Func<IntPtr> consoleWindowHandleProvider = () => _parentHandle;

            string[] scopes = new[]
                {
                    "https://management.core.windows.net//.default"
                };

            var builder = PublicClientApplicationBuilder
                .Create("04f0c124-f2bc-4f59-8241-bf6df9866bbd")
                .WithAuthority("https://login.microsoftonline.com/organizations")
                .WithParentActivityOrWindow(consoleWindowHandleProvider)
                .WithBrokerPreview();

            var pca = builder.Build();
            try
            {
                var result = await pca.AcquireTokenInteractive(scopes)
                    .WithAccount(PublicClientApplication.OperatingSystemAccount)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                Console.WriteLine(result.AccessToken);
            }
            catch (MsalClientException ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.Read();
        }
    }
}
