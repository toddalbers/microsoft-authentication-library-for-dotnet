// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client.Core;
using Microsoft.Identity.Json.Utilities;

namespace Microsoft.Identity.Client.Http
{
    internal interface IHttpManager
    {
        long LastRequestDurationInMs { get; }

        Task<HttpResponse> SendPostAsync(
            Uri endpoint,
            IDictionary<string, string> headers,
            IDictionary<string, string> bodyParameters,
            ILoggerAdapter logger,
            CancellationToken cancellationToken = default);

        Task<HttpResponse> SendPostAsync(
            Uri endpoint,
            IDictionary<string, string> headers,
            HttpContent body,
            ILoggerAdapter logger,
            CancellationToken cancellationToken = default);

        Task<HttpResponse> SendGetAsync(
            Uri endpoint,
            IDictionary<string, string> headers,
            ILoggerAdapter logger,
            bool retry = true,
            CancellationToken cancellationToken = default);

        Task<HttpResponse> SendPostForceResponseAsync(
            Uri uri,
            Dictionary<string, string> headers,
            StringContent body,
            ILoggerAdapter logger,
            CancellationToken cancellationToken = default);
    }

  
}
