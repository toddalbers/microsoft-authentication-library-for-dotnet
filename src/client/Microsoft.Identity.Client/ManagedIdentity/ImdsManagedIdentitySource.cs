// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client.Core;
using Microsoft.Identity.Client.Extensibility;
using Microsoft.Identity.Client.Http;
using Microsoft.Identity.Client.Internal;
using Microsoft.Identity.Client.Utils;
using Microsoft.IdentityModel.Abstractions;

namespace Microsoft.Identity.Client.ManagedIdentity
{
    internal class ImdsManagedIdentitySource : ManagedIdentitySource
    {
        // IMDS constants. Docs for IMDS are available here https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/how-to-use-vm-token#get-a-token-using-http
        private static readonly Uri s_imdsEndpoint = new Uri("http://169.254.169.254/metadata/identity/oauth2/token");
        internal const string imddsTokenPath = "/metadata/identity/oauth2/token";

        private const string ImdsApiVersion = "2018-02-01";
        private const string DefaultMessage = "Service request failed.";

        internal const string IdentityUnavailableError = "ManagedIdentityCredential authentication unavailable. The requested identity has not been assigned to this resource.";
        internal const string NoResponseError = "ManagedIdentityCredential authentication unavailable. No response received from the managed identity endpoint.";
        internal const string TimeoutError = "ManagedIdentityCredential authentication unavailable. The request to the managed identity endpoint timed out.";
        internal const string GatewayError = "ManagedIdentityCredential authentication unavailable. The request failed due to a gateway error.";
        internal const string AggregateError = "ManagedIdentityCredential authentication unavailable. Multiple attempts failed to obtain a token from the managed identity endpoint.";

        private readonly string _clientId;
        private readonly string _resourceId;
        private readonly Uri _imdsEndpoint;

        private TimeSpan? _imdsNetworkTimeout;

        internal ImdsManagedIdentitySource(RequestContext requestContext) : base(requestContext)
        {
            if (!string.IsNullOrEmpty(EnvironmentVariables.PodIdentityEndpoint))
			{
				var builder = new UriBuilder(EnvironmentVariables.PodIdentityEndpoint);
            	builder.Path = imddsTokenPath;
                _imdsEndpoint = builder.Uri;
			}
			else
			{
            	_imdsEndpoint = s_imdsEndpoint;
			}
        }

        protected override ManagedIdentityRequest CreateRequest(string[] scopes)
        {
            // covert the scopes to a resource string
            string resource = ScopeUtilities.ScopesToResource(scopes);

            ManagedIdentityRequest request = new ManagedIdentityRequest(HttpMethod.Get, _imdsEndpoint);
            IDictionary<string, string> queryParams = new Dictionary<string, string>();

            request.Headers.Add("Metadata", "true");
            queryParams["api-version"] = ImdsApiVersion;
            queryParams["resource"] = resource;

            if (!string.IsNullOrEmpty(_clientId))
            {
                queryParams[Constants.ManagedIdentityClientId] = _clientId;
            }
            if (!string.IsNullOrEmpty(_resourceId))
            {
                queryParams[Constants.ManagedIdentityResourceId] = _resourceId;
            }

            request.UriBuilder.AppendQueryParameters(queryParams);

            return request;
        }

        public async override Task<ManagedIdentityResponse> AuthenticateAsync(AppTokenProviderParameters parameters, CancellationToken cancellationToken)
        {
            try
            {
                return await base.AuthenticateAsync(parameters, cancellationToken).ConfigureAwait(false);
            }
            catch (MsalServiceException e) when (e.ErrorCode == MsalError.ManagedIdentityFailedResponse)
            {
                _requestContext.Logger.Error(NoResponseError);
                throw;
            }
            catch (TaskCanceledException e)
            {
                // Should we capture this or just throw?
                throw;
            }
            catch (AggregateException e)
            {
                // try to capture if multiple attempts failed for imds
                throw new MsalServiceException(MsalError.ManagedIdentityFailedResponse, AggregateError, e);
            }
        }

        protected override ManagedIdentityResponse HandleResponse(
            AppTokenProviderParameters parameters, 
            HttpResponse response)
        {
            // if we got a response from IMDS we can stop limiting the network timeout
            _imdsNetworkTimeout = null;

            // handle error status codes indicating managed identity is not available
            var baseMessage = response.StatusCode switch
            {
                HttpStatusCode.BadRequest => IdentityUnavailableError,
                HttpStatusCode.BadGateway => GatewayError,
                HttpStatusCode.GatewayTimeout => GatewayError,
                _ => default(string)
            };

            if (baseMessage != null)
            {
                string message = CreateRequestFailedMessage(response, baseMessage);

                var errorContentMessage = GetMessageFromResponse(response);

                if (errorContentMessage != null)
                {
                    message = message + Environment.NewLine + errorContentMessage;
                }

                throw new MsalServiceException(MsalError.ManagedIdentityFailedResponse, message);
            }

            return base.HandleResponse(parameters, response);
        }

        public string CreateRequestFailedMessage(HttpResponse response, string message)
        { 
            
            return CreateRequestFailedMessageWithContent(response, message, response.Body);
        }

        internal static string CreateRequestFailedMessageWithContent(HttpResponse response, string message, string? content)
        {
            StringBuilder messageBuilder = new StringBuilder();

            messageBuilder
                .AppendLine(message ?? DefaultMessage)
                .Append("Status: ")
                .Append(response.StatusCode.ToString());

            if (content != null)
            {
                messageBuilder
                    .AppendLine()
                    .AppendLine("Content:")
                    .AppendLine(content);
            }

            messageBuilder
                .AppendLine()
                .AppendLine("Headers:");

            foreach (var header in response.HeadersAsDictionary)
            {
                //string headerValue = sanitizer.SanitizeHeader(responseHeader.Name, responseHeader.Value);
                messageBuilder.AppendLine($"{header.Key}: {header.Value}");
            }

            return messageBuilder.ToString();
        }
    }
}
