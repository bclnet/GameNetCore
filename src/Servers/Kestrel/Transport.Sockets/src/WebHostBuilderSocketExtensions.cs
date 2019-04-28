// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Contoso.GameNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.GameNetCore.Hosting
{
    public static class GameHostBuilderSocketExtensions
    {
        /// <summary>
        /// Specify Sockets as the transport to be used by Kestrel.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Microsoft.AspNetCore.Hosting.IGameHostBuilder to configure.
        /// </param>
        /// <returns>
        /// The Microsoft.AspNetCore.Hosting.IGameHostBuilder.
        /// </returns>
        public static IGameHostBuilder UseSockets(this IGameHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<ITransportFactory, SocketTransportFactory>();
            });
        }

        /// <summary>
        /// Specify Sockets as the transport to be used by Kestrel.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Microsoft.AspNetCore.Hosting.IGameHostBuilder to configure.
        /// </param>
        /// <param name="configureOptions">
        /// A callback to configure Libuv options.
        /// </param>
        /// <returns>
        /// The Microsoft.AspNetCore.Hosting.IGameHostBuilder.
        /// </returns>
        public static IGameHostBuilder UseSockets(this IGameHostBuilder hostBuilder, Action<SocketTransportOptions> configureOptions)
        {
            return hostBuilder.UseSockets().ConfigureServices(services =>
            {
                services.Configure(configureOptions);
            });
        }
    }
}
