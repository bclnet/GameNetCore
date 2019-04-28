﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Hosting.Server;
using Contoso.GameNetCore.Server.HttpSys;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.GameNetCore.Hosting
{
    public static class WebHostBuilderHttpSysExtensions
    {
        /// <summary>
        /// Specify HttpSys as the server to be used by the web host.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Microsoft.AspNetCore.Hosting.IWebHostBuilder to configure.
        /// </param>
        /// <returns>
        /// The Microsoft.AspNetCore.Hosting.IWebHostBuilder.
        /// </returns>
        public static IWebHostBuilder UseHttpSys(this IWebHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(services => {
                services.AddSingleton<IServer, MessagePump>();
                services.AddAuthenticationCore();
            });
        }

        /// <summary>
        /// Specify HttpSys as the server to be used by the web host.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Microsoft.AspNetCore.Hosting.IWebHostBuilder to configure.
        /// </param>
        /// <param name="options">
        /// A callback to configure HttpSys options.
        /// </param>
        /// <returns>
        /// The Microsoft.AspNetCore.Hosting.IWebHostBuilder.
        /// </returns>
        public static IWebHostBuilder UseHttpSys(this IWebHostBuilder hostBuilder, Action<HttpSysOptions> options)
        {
            return hostBuilder.UseHttpSys().ConfigureServices(services =>
            {
                services.Configure(options);
            });
        }
    }
}
