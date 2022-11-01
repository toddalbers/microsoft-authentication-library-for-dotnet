// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Test.Integration.NetFx.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Identity.Test.Integration.NetFx.HeadlessTests
{
    [TestClass]
    public class ManagedIdentityTests
    {
        [TestMethod]
        public async Task AzureFunction_TestAsync()
        {
            ProxyHttpManager proxyHttpManager = new ProxyHttpManager("https://msitest.azurewebsites.com/azure-function");

            // TODO: this needs to be built
            // ProxyEnvironmenntVariableManager = new ProxyEnvironmenntVariableManager("https://msitest.azurewebsites.com/azure-function");

            // TODO: inject the env variable service, see https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/main/src/client/Microsoft.Identity.Client/AppConfig/ApplicationConfiguration.cs#L190 for an example
            var cca = ConfidentialClientApplicationBuilder
               .Create("clientid")
               .WithHttpManager(proxyHttpManager)
               // .WithManagedIdentity(...)
               .Build();

        }
    }
}
