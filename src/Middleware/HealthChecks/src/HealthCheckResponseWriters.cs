// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Contoso.GameNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Contoso.GameNetCore.Diagnostics.HealthChecks
{
    internal static class HealthCheckResponseWriters
    {
        public static Task WriteMinimalPlaintext(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = "text/plain";
            return httpContext.Response.WriteAsync(result.Status.ToString());
        }
    }
}
