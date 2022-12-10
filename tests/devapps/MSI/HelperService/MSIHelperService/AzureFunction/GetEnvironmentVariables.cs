﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

//#r "Newtonsoft.Json" //uncomment this on the function app 

using System.Net;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

/// <summary>
/// Name space is not required while updating this code in the function app
/// </summary>
namespace MSIHelperService.AzureFunction
{
    /// <summary>
    /// Gets the environment variables from the function app 
    /// </summary>
    public class GetEnvironmentVariables
    {
        private static Dictionary<string, string> _envVariables => new Dictionary<string, string>();

        public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["variableName"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);
            dynamic? data = JsonConvert.DeserializeObject(requestBody);
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            name = name ?? data?.variableName;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

#pragma warning disable CA2254 // Template should be a static expression
            log.LogInformation($"Querysting value for  variableName is : { name } ");
#pragma warning restore CA2254 // Template should be a static expression

            if (string.IsNullOrEmpty(name))
            {
                Dictionary<string, string?> _envVariables = new Dictionary<string, string?>();
                _envVariables.Add("IDENTITY_HEADER", Environment.GetEnvironmentVariable("IDENTITY_HEADER"));
                _envVariables.Add("IDENTITY_ENDPOINT", Environment.GetEnvironmentVariable("IDENTITY_ENDPOINT"));
                _envVariables.Add("IDENTITY_API_VERSION", Environment.GetEnvironmentVariable("IDENTITY_API_VERSION"));

                log.LogInformation("Returning All Environment Variables");
                return new OkObjectResult(_envVariables);
            }
            else
            {
                string? responseMessage = Environment.GetEnvironmentVariable(name);
#pragma warning disable CA2254 // Template should be a static expression
                log.LogInformation($"Returning Environment Variables Based on the variable name : { name } ");
#pragma warning restore CA2254 // Template should be a static expression
                return new OkObjectResult(responseMessage);
            }
        }
    }
}
