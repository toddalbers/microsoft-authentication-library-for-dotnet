﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client.AuthScheme.SSHCertificates;
using Microsoft.Identity.Client.TelemetryCore.Internal.Events;

namespace Microsoft.Identity.Client.SSHCertificates
{
    /// <summary>
    /// Extensions that add support for SSH certificates
    /// </summary>
    public static class SSHExtensions
    {
        /// <summary>
        /// Instructs AAD to return an SSH certificate instead of a Bearer token. Attempts to retrieve
        /// the certificate from the token cache, and if one is not found, attempts to acquire one silently, 
        /// using the refresh token. See https://aka.ms/msal-net-ssh for details.
        /// </summary>
        /// <remarks>
        /// The same keyID must be used to distinguish between various 
        /// </remarks>
        /// <param name="builder">Silent authentication builder</param>
        /// <param name="publicKeyJwk">The public SSH key in JWK format (https://tools.ietf.org/html/rfc7517). 
        /// Currently only RSA is supported, and the JWK should contain only the RSA modulus and exponent</param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public static AcquireTokenSilentParameterBuilder WithSSHCertificateAuthenticationScheme(
            this AcquireTokenSilentParameterBuilder builder,
            string publicKeyJwk,
            string keyId)
        {
            builder.CommonParameters.AuthenticationScheme = new SSHCertAuthenticationScheme(keyId, publicKeyJwk);
            return builder;
        }
    }
}
