// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Contoso.GameNetCore.Hosting.TestSites
{
    public class StartupShutdown
    {
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IHostApplicationLifetime lifetime)
        {
            lifetime.ApplicationStarted.Register(() =>
            {
                Console.WriteLine("Started");
            });
            lifetime.ApplicationStopping.Register(() =>
            {
                Console.WriteLine("Stopping firing");
                System.Threading.Thread.Sleep(200);
                Console.WriteLine("Stopping end");
            });
            lifetime.ApplicationStopped.Register(() =>
            {
                Console.WriteLine("Stopped firing");
                System.Threading.Thread.Sleep(200);
                Console.WriteLine("Stopped end");
            });

            app.Run(context =>
            {
                return context.Response.WriteAsync("Hello World");
            });
        }
    }
}
