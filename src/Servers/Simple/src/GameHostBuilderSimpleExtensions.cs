// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Hosting.Server;
using Contoso.GameNetCore.Server.Simple.Core;
using Contoso.GameNetCore.Server.Simple.Core.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Contoso.GameNetCore.Hosting
{
    public static class GameHostBuilderSingleExtensions
    {
        /// <summary>
        /// Specify Simple as the server to be used by the game host.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder to configure.
        /// </param>
        /// <returns>
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder.
        /// </returns>
        public static IGameHostBuilder UseSimple(this IGameHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddTransient<IConfigureOptions<SimpleServerOptions>, SimpleServerOptionsSetup>();
                services.AddSingleton<IServer, SimpleServer>();
            });
        }

        /// <summary>
        /// Specify Simple as the server to be used by the game host.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder to configure.
        /// </param>
        /// <param name="options">
        /// A callback to configure Simple options.
        /// </param>
        /// <returns>
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder.
        /// </returns>
        public static IGameHostBuilder UseSimple(this IGameHostBuilder hostBuilder, Action<SimpleServerOptions> options) => hostBuilder.UseSimple().ConfigureSimple(options);

        /// <summary>
        /// Configures Simple options but does not register an IServer. See <see cref="UseSimple(IGameHostBuilder)"/>.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder to configure.
        /// </param>
        /// <param name="options">
        /// A callback to configure Simple options.
        /// </param>
        /// <returns>
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder.
        /// </returns>
        public static IGameHostBuilder ConfigureSimple(this IGameHostBuilder hostBuilder, Action<SimpleServerOptions> options) =>
            hostBuilder.ConfigureServices(services =>
            {
                services.Configure(options);
            });

        /// <summary>
        /// Specify Simple as the server to be used by the game host.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder to configure.
        /// </param>
        /// <param name="configureOptions">A callback to configure Simple options.</param>
        /// <returns>
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder.
        /// </returns>
        public static IGameHostBuilder UseSimple(this IGameHostBuilder hostBuilder, Action<GameHostBuilderContext, SimpleServerOptions> configureOptions) =>
            hostBuilder.UseSimple().ConfigureSimple(configureOptions);

        /// <summary>
        /// Configures Simple options but does not register an IServer. See <see cref="UseSimple(IGameHostBuilder)"/>.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder to configure.
        /// </param>
        /// <param name="configureOptions">A callback to configure Simple options.</param>
        /// <returns>
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder.
        /// </returns>
        public static IGameHostBuilder ConfigureSimple(this IGameHostBuilder hostBuilder, Action<GameHostBuilderContext, SimpleServerOptions> configureOptions)
        {
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.Configure<SimpleServerOptions>(options =>
                {
                    configureOptions(context, options);
                });
            });
        }
    }
}
