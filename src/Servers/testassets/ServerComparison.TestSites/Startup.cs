// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.InteropServices;
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ServerComparison.TestSites
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.Run(ctx =>
            {
                return ctx.Response.WriteAsync("Hello World " + RuntimeInformation.ProcessArchitecture);
            });
        }
    }
}