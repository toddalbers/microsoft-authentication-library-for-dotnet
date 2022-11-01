// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client.Core;
using Microsoft.Identity.Client.Http;

namespace Microsoft.Identity.Test.Integration.NetFx.Infrastructure
{
    /// <summary>
    ///  This implementation of IHttpManager proxies the HTTP request to MSIWebAPI. 
    /// </summary>
    public class ProxyHttpManager : IHttpManager
    {
        private readonly string _testWebServiceEndpoint;
        private static HttpClient s_httpClient = new HttpClient();

        public ProxyHttpManager(string testWebServiceEndpoint)
        {
            _testWebServiceEndpoint = testWebServiceEndpoint;
        }

        public long LastRequestDurationInMs => 0;

        long IHttpManager.LastRequestDurationInMs => throw new NotImplementedException();

        async Task<HttpResponse> SendGetAsync(
            Uri endpoint, 
            IDictionary<string, string> headers, 
            ILoggerAdapter logger, 
            bool retry, 
            CancellationToken cancellationToken)
        {

            // TODO: are headers needed?
            if (!headers.Any())
                throw new NotImplementedException("Not expecting to send custom headers to MSI endpoint");

            var encodedUri = Convert.ToBase64String(Encoding.UTF8.GetBytes(endpoint.AbsoluteUri));
            HttpResponseMessage result = await s_httpClient.SendAsync(new HttpRequestMessage()
            {

                RequestUri = new Uri(_testWebServiceEndpoint + "/GetRemoteHttpResponse?uri" + encodedUri)
            }).ConfigureAwait(false);

            HttpResponse response = new HttpResponse();

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                response.StatusCode = result.StatusCode;
                // TODO: Body may contain sensitive info like tokens
                // Either secure the connection between the test and the web service, 
                // or sanitize the responses
                response.Body = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            // TODO: failures should be handled


            return response;
        }

      

        Task<HttpResponse> IHttpManager.SendPostAsync(Uri endpoint, IDictionary<string, string> headers, IDictionary<string, string> bodyParameters, ILoggerAdapter logger, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<HttpResponse> IHttpManager.SendPostAsync(Uri endpoint, IDictionary<string, string> headers, HttpContent body, ILoggerAdapter logger, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<HttpResponse> IHttpManager.SendGetAsync(Uri endpoint, IDictionary<string, string> headers, ILoggerAdapter logger, bool retry, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<HttpResponse> IHttpManager.SendPostForceResponseAsync(Uri uri, Dictionary<string, string> headers, StringContent body, ILoggerAdapter logger, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
