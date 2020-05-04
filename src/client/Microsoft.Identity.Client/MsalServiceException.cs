﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Net.Http.Headers;
using Microsoft.Identity.Client.Utils;
using Microsoft.Identity.Json.Linq;

namespace Microsoft.Identity.Client
{

    /// <summary>
    /// Exception type thrown when service returns an error response or other networking errors occur.
    /// For more details, see https://aka.ms/msal-net-exceptions
    /// </summary>
    public class MsalServiceException : MsalException
    {
        private const string ClaimsKey = "claims";
        private const string ResponseBodyKey = "response_body";
        private const string CorrelationIdKey = "correlation_id";
        private const string SubErrorKey = "sub_error";

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the exception class with a specified
        /// error code, error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="errorCode">
        /// The protocol error code returned by the service or generated by client. This is the code you
        /// can rely on for exception handling.
        /// </param>
        /// <param name="errorMessage">The error message that explains the reason for the exception.</param>
        public MsalServiceException(string errorCode, string errorMessage)
            : base(errorCode, errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }
        }

        /// <summary>
        /// Initializes a new instance of the exception class with a specified
        /// error code, error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="errorCode">
        /// The protocol error code returned by the service or generated by the client. This is the code you
        /// can rely on for exception handling.
        /// </param>
        /// <param name="errorMessage">The error message that explains the reason for the exception.</param>
        /// <param name="statusCode">Status code of the resposne received from the service.</param>
        public MsalServiceException(string errorCode, string errorMessage, int statusCode)
            : this(errorCode, errorMessage)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the exception class with a specified
        /// error code, error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="errorCode">
        /// The protocol error code returned by the service or generated by the client. This is the code you
        /// can rely on for exception handling.
        /// </param>
        /// <param name="errorMessage">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference if no inner
        /// exception is specified.
        /// </param>
        public MsalServiceException(string errorCode, string errorMessage,
            Exception innerException)
            : base(errorCode, errorMessage, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the exception class with a specified
        /// error code, error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="errorCode">
        /// The protocol error code returned by the service or generated by the client. This is the code you
        /// can rely on for exception handling.
        /// </param>
        /// <param name="errorMessage">The error message that explains the reason for the exception.</param>
        /// <param name="statusCode">HTTP status code of the resposne received from the service.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference if no inner
        /// exception is specified.
        /// </param>
        public MsalServiceException(string errorCode, string errorMessage, int statusCode,
            Exception innerException)
            : base(
                errorCode, errorMessage, innerException)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the exception class with a specified
        /// error code, error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="errorCode">
        /// The protocol error code returned by the service or generated by the client. This is the code you
        /// can rely on for exception handling.
        /// </param>
        /// <param name="errorMessage">The error message that explains the reason for the exception.</param>
        /// <param name="statusCode">The status code of the request.</param>
        /// <param name="claims">The claims challenge returned back from the service.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference if no inner
        /// exception is specified.
        /// </param>
        public MsalServiceException(
            string errorCode,
            string errorMessage,
            int statusCode,
            string claims,
            Exception innerException)
            : this(errorCode, errorMessage, statusCode, innerException)
        {
            Claims = claims;
        }

        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the status code returned from http layer. This status code is either the <c>HttpStatusCode</c> in the inner
        /// <see cref="System.Net.Http.HttpRequestException"/> response or the the NavigateError Event Status Code in a browser based flow (See
        /// http://msdn.microsoft.com/en-us/library/bb268233(v=vs.85).aspx).
        /// You can use this code for purposes such as implementing retry logic or error investigation.
        /// </summary>
        public int StatusCode { get; internal set; } = 0;

#if !DESKTOP
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#endif
        /// <summary>
        /// Additional claims requested by the service. When this property is not null or empty, this means that the service requires the user to
        /// provide additional claims, such as doing two factor authentication. The are two cases:
        /// <list type="bullent">
        /// <item><description>
        /// If your application is a <see cref="IPublicClientApplication"/>, you should just call <see cref="IPublicClientApplication.AcquireTokenInteractive(System.Collections.Generic.IEnumerable{string})"/>
        /// and add the <see cref="AbstractAcquireTokenParameterBuilder{T}.WithClaims(string)"/> modifier.
        /// </description></item>
        /// <item>><description>If your application is a <see cref="IConfidentialClientApplication"/>, (therefore doing the On-Behalf-Of flow), you should throw an Http unauthorize
        /// exception with a message containing the claims</description></item>
        /// </list>
        /// For more details see https://aka.ms/msal-net-claim-challenge
        /// </summary>
        public string Claims { get; internal set; }
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved

        /// <summary>
        /// Raw response body received from the server.
        /// </summary>
        public string ResponseBody { get; internal set; }


        /// <summary>
        /// Contains the http headers from the server response that indicated an error.
        /// </summary>
        /// <remarks>
        /// When the server returns a 429 Too Many Requests error, a Retry-After should be set. It is important to read and respect the
        /// time specified in the Retry-After header to avoid a retry storm.
        /// </remarks>
        public HttpResponseHeaders Headers { get; internal set; }

        /// <summary>
        /// An ID that can used to piece up a single authentication flow.
        /// </summary>
        public string CorrelationId { get; internal set; }

        #endregion

        /// <remarks>
        /// The suberror should not be exposed for public consumption yet, as STS needs to do some work
        /// first.
        /// </remarks>
        internal string SubError { get; set; }

        /// <summary>
        /// As per discussion with Evo, AAD 
        /// </summary>
        internal bool IsAadUnavailable()
        {
            return 
                StatusCode == 429 || // "Too Many Requests", does not mean AAD is down
                StatusCode >= 500 || 
                string.Equals(ErrorCode, MsalError.RequestTimeout, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>A string representation of the current exception.</returns>
        public override string ToString()
        {
            return base.ToString() + string.Format(
                CultureInfo.InvariantCulture,
                "\n\tStatusCode: {0} \n\tResponseBody: {1} \n\tHeaders: {2}",
                StatusCode,
                ResponseBody,
                Headers);
        }

        #region Serialization


        // DEPRECATE / OBSOLETE - this functionality is not used and should be removed in a next major version

        internal override void PopulateJson(JObject jobj)
        {
            base.PopulateJson(jobj);

            jobj[ClaimsKey] = Claims;
            jobj[ResponseBodyKey] = ResponseBody;
            jobj[CorrelationIdKey] = CorrelationId;
            jobj[SubErrorKey] = SubError;
        }

        internal override void PopulateObjectFromJson(JObject jobj)
        {
            base.PopulateObjectFromJson(jobj);

            Claims = JsonUtils.GetExistingOrEmptyString(jobj, ClaimsKey);
            ResponseBody = JsonUtils.GetExistingOrEmptyString(jobj, ResponseBodyKey);
            CorrelationId = JsonUtils.GetExistingOrEmptyString(jobj, CorrelationIdKey);
            SubError = JsonUtils.GetExistingOrEmptyString(jobj, SubErrorKey);
        }
        #endregion
    }
}
