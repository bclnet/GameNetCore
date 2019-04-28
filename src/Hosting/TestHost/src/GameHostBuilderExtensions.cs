// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Contoso.GameNetCore.Hosting;
using Contoso.GameNetCore.Hosting.Internal;
using Contoso.GameNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.GameNetCore.TestHost
{
    public static class GameHostBuilderExtensions
    {
        public static IGameHostBuilder UseTestServer(this IGameHostBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddSingleton<IServer, TestServer>();
            });
        }

        public static IGameHostBuilder ConfigureTestServices(this IGameHostBuilder gameHostBuilder, Action<IServiceCollection> servicesConfiguration)
        {
            if (gameHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(gameHostBuilder));
            }

            if (servicesConfiguration == null)
            {
                throw new ArgumentNullException(nameof(servicesConfiguration));
            }

            if (gameHostBuilder.GetType().Name.Equals("GenericGameHostBuilder"))
            {
                // Generic host doesn't need to do anything special here since there's only one container.
                gameHostBuilder.ConfigureServices(servicesConfiguration);
            }
            else
            {
                gameHostBuilder.ConfigureServices(
                    s => s.AddSingleton<IStartupConfigureServicesFilter>(
                        new ConfigureTestServicesStartupConfigureServicesFilter(servicesConfiguration)));
            }

            return gameHostBuilder;
        }

        public static IGameHostBuilder ConfigureTestContainer<TContainer>(this IGameHostBuilder gameHostBuilder, Action<TContainer> servicesConfiguration)
        {
            if (gameHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(gameHostBuilder));
            }

            if (servicesConfiguration == null)
            {
                throw new ArgumentNullException(nameof(servicesConfiguration));
            }

            gameHostBuilder.ConfigureServices(
                s => s.AddSingleton<IStartupConfigureContainerFilter<TContainer>>(
                    new ConfigureTestServicesStartupConfigureContainerFilter<TContainer>(servicesConfiguration)));

            return gameHostBuilder;
        }

        public static IGameHostBuilder UseSolutionRelativeContentRoot(
            this IGameHostBuilder builder,
            string solutionRelativePath,
            string solutionName = "*.sln")
        {
            return builder.UseSolutionRelativeContentRoot(solutionRelativePath, AppContext.BaseDirectory, solutionName);
        }

        public static IGameHostBuilder UseSolutionRelativeContentRoot(
            this IGameHostBuilder builder,
            string solutionRelativePath,
            string applicationBasePath,
            string solutionName = "*.sln")
        {
            if (solutionRelativePath == null)
            {
                throw new ArgumentNullException(nameof(solutionRelativePath));
            }

            if (applicationBasePath == null)
            {
                throw new ArgumentNullException(nameof(applicationBasePath));
            }

            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                var solutionPath = Directory.EnumerateFiles(directoryInfo.FullName, solutionName).FirstOrDefault();
                if (solutionPath != null)
                {
                    builder.UseContentRoot(Path.GetFullPath(Path.Combine(directoryInfo.FullName, solutionRelativePath)));
                    return builder;
                }

                directoryInfo = directoryInfo.Parent;
            }
            while (directoryInfo.Parent != null);

            throw new InvalidOperationException($"Solution root could not be located using application root {applicationBasePath}.");
        }

        private class ConfigureTestServicesStartupConfigureServicesFilter : IStartupConfigureServicesFilter
        {
            private readonly Action<IServiceCollection> _servicesConfiguration;

            public ConfigureTestServicesStartupConfigureServicesFilter(Action<IServiceCollection> servicesConfiguration)
            {
                if (servicesConfiguration == null)
                {
                    throw new ArgumentNullException(nameof(servicesConfiguration));
                }

                _servicesConfiguration = servicesConfiguration;
            }

            public Action<IServiceCollection> ConfigureServices(Action<IServiceCollection> next) =>
                serviceCollection =>
                {
                    next(serviceCollection);
                    _servicesConfiguration(serviceCollection);
                };
        }

        private class ConfigureTestServicesStartupConfigureContainerFilter<TContainer> : IStartupConfigureContainerFilter<TContainer>
        {
            private readonly Action<TContainer> _servicesConfiguration;

            public ConfigureTestServicesStartupConfigureContainerFilter(Action<TContainer> containerConfiguration)
            {
                if (containerConfiguration == null)
                {
                    throw new ArgumentNullException(nameof(containerConfiguration));
                }

                _servicesConfiguration = containerConfiguration;
            }

            public Action<TContainer> ConfigureContainer(Action<TContainer> next) =>
                containerBuilder =>
                {
                    next(containerBuilder);
                    _servicesConfiguration(containerBuilder);
                };
        }
    }
}
