// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Hosting;
using Contoso.GameNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.GameNetCore.GameSockets.Test
{
    public class KestrelGameSocketHelpers
    {
        public static IDisposable CreateServer(ILoggerFactory loggerFactory, out int port, Func<HttpContext, Task> app, Action<GameSocketOptions> configure = null)
        {
            configure = configure ?? (o => { });
            Action<IApplicationBuilder> startup = builder =>
            {
                builder.Use(async (ct, next) =>
                {
                    try
                    {
                        // Kestrel does not return proper error responses:
                        // https://github.com/aspnet/KestrelHttpServer/issues/43
                        await next();
                    }
                    catch (Exception ex)
                    {
                        if (ct.Response.HasStarted)
                        {
                            throw;
                        }

                        ct.Response.StatusCode = 500;
                        ct.Response.Headers.Clear();
                        await ct.Response.WriteAsync(ex.ToString());
                    }
                });
                builder.UseGameSockets();
                builder.Run(c => app(c));
            };

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection();
            var config = configBuilder.Build();
            config["server.urls"] = $"http://127.0.0.1:0";

            var host = new GameHostBuilder()
                .ConfigureServices(s =>
                {
                    s.AddGameSockets(configure);
                    s.AddSingleton(loggerFactory);
                })
                .UseConfiguration(config)
                .UseKestrel()
                .Configure(startup)
                .Build();

            host.Start();
            port = host.GetPort();

            return host;
        }
    }
}

