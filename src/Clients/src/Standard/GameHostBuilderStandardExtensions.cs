// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Hosting.Client;
using Contoso.GameNetCore.Client.Standard.Core;
using Contoso.GameNetCore.Client.Standard.Core.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Contoso.GameNetCore.Hosting
{
    public static class GameHostBuilderStandardExtensions
    {
        /// <summary>
        /// Specify Standard as the client to be used by the game host.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder to configure.
        /// </param>
        /// <returns>
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder.
        /// </returns>
        public static IGameHostBuilder UseStandard(this IGameHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddTransient<IConfigureOptions<StandardClientOptions>, StandardClientOptionsSetup>();
                services.AddSingleton<IClient, StandardClient>();
            });
        }

        /// <summary>
        /// Specify Standard as the client to be used by the game host.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder to configure.
        /// </param>
        /// <param name="options">
        /// A callback to configure Standard options.
        /// </param>
        /// <returns>
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder.
        /// </returns>
        public static IGameHostBuilder UseStandard(this IGameHostBuilder hostBuilder, Action<StandardClientOptions> options) => hostBuilder.UseStandard().ConfigureStandard(options);

        /// <summary>
        /// Configures Standard options but does not register an IClient. See <see cref="UseStandard(IGameHostBuilder)"/>.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder to configure.
        /// </param>
        /// <param name="options">
        /// A callback to configure Standard options.
        /// </param>
        /// <returns>
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder.
        /// </returns>
        public static IGameHostBuilder ConfigureStandard(this IGameHostBuilder hostBuilder, Action<StandardClientOptions> options) =>
            hostBuilder.ConfigureServices(services =>
            {
                services.Configure(options);
            });

        /// <summary>
        /// Specify Standard as the client to be used by the game host.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder to configure.
        /// </param>
        /// <param name="configureOptions">A callback to configure Standard options.</param>
        /// <returns>
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder.
        /// </returns>
        public static IGameHostBuilder UseStandard(this IGameHostBuilder hostBuilder, Action<GameHostBuilderContext, StandardClientOptions> configureOptions) =>
            hostBuilder.UseStandard().ConfigureStandard(configureOptions);

        /// <summary>
        /// Configures Standard options but does not register an IClient. See <see cref="UseStandard(IGameHostBuilder)"/>.
        /// </summary>
        /// <param name="hostBuilder">
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder to configure.
        /// </param>
        /// <param name="configureOptions">A callback to configure Standard options.</param>
        /// <returns>
        /// The Contoso.GameNetCore.Hosting.IGameHostBuilder.
        /// </returns>
        public static IGameHostBuilder ConfigureStandard(this IGameHostBuilder hostBuilder, Action<GameHostBuilderContext, StandardClientOptions> configureOptions)
        {
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.Configure<StandardClientOptions>(options =>
                {
                    configureOptions(context, options);
                });
            });
        }
    }
}
