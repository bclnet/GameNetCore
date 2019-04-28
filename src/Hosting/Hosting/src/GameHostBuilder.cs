// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Hosting.Builder;
using Contoso.GameNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Hosting;
#if NET2
using IHostEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;
#endif

namespace Contoso.GameNetCore.Hosting
{
    /// <summary>
    /// A builder for <see cref="IGameHost"/>
    /// </summary>
    public class GameHostBuilder : IGameHostBuilder
    {
        readonly HostingEnvironment _hostingEnvironment;
        Action<GameHostBuilderContext, IServiceCollection> _configureServices;

        IConfiguration _config;
        GameHostOptions _options;
        GameHostBuilderContext _context;
        bool _gameHostBuilt;
        Action<GameHostBuilderContext, IConfigurationBuilder> _configureAppConfigurationBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameHostBuilder"/> class.
        /// </summary>
        public GameHostBuilder()
        {
            _hostingEnvironment = new HostingEnvironment();

            _config = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "GAMENETCORE_")
                .Build();

            if (string.IsNullOrEmpty(GetSetting(GameHostDefaults.EnvironmentKey)))
                // Try adding legacy environment keys, never remove these.
                UseSetting(GameHostDefaults.EnvironmentKey, Environment.GetEnvironmentVariable("Hosting:Environment")
                    ?? Environment.GetEnvironmentVariable("GAMENET_ENV"));

            if (string.IsNullOrEmpty(GetSetting(GameHostDefaults.ServerUrlsKey)))
                // Try adding legacy url key, never remove this.
                UseSetting(GameHostDefaults.ServerUrlsKey, Environment.GetEnvironmentVariable("GAMENETCORE_SERVER.URLS"));

            _context = new GameHostBuilderContext
            {
                Configuration = _config
            };
        }

        /// <summary>
        /// Get the setting value from the configuration.
        /// </summary>
        /// <param name="key">The key of the setting to look up.</param>
        /// <returns>The value the setting currently contains.</returns>
        public string GetSetting(string key) =>
            _config[key];

        /// <summary>
        /// Add or replace a setting in the configuration.
        /// </summary>
        /// <param name="key">The key of the setting to add or replace.</param>
        /// <param name="value">The value of the setting to add or replace.</param>
        /// <returns>The <see cref="IGameHostBuilder"/>.</returns>
        public IGameHostBuilder UseSetting(string key, string value)
        {
            _config[key] = value;
            return this;
        }

        /// <summary>
        /// Adds a delegate for configuring additional services for the host or game application. This may be called
        /// multiple times.
        /// </summary>
        /// <param name="configureServices">A delegate for configuring the <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IGameHostBuilder"/>.</returns>
        public IGameHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            if (configureServices == null)
                throw new ArgumentNullException(nameof(configureServices));
            return ConfigureServices((_, services) => configureServices(services));
        }

        /// <summary>
        /// Adds a delegate for configuring additional services for the host or game application. This may be called
        /// multiple times.
        /// </summary>
        /// <param name="configureServices">A delegate for configuring the <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IGameHostBuilder"/>.</returns>
        public IGameHostBuilder ConfigureServices(Action<GameHostBuilderContext, IServiceCollection> configureServices)
        {
            _configureServices += configureServices;
            return this;
        }

        /// <summary>
        /// Adds a delegate for configuring the <see cref="IConfigurationBuilder"/> that will construct an <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="configureDelegate">The delegate for configuring the <see cref="IConfigurationBuilder" /> that will be used to construct an <see cref="IConfiguration" />.</param>
        /// <returns>The <see cref="IGameHostBuilder"/>.</returns>
        /// <remarks>
        /// The <see cref="IConfiguration"/> and <see cref="ILoggerFactory"/> on the <see cref="GameHostBuilderContext"/> are uninitialized at this stage.
        /// The <see cref="IConfigurationBuilder"/> is pre-populated with the settings of the <see cref="IGameHostBuilder"/>.
        /// </remarks>
        public IGameHostBuilder ConfigureAppConfiguration(Action<GameHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            _configureAppConfigurationBuilder += configureDelegate;
            return this;
        }

        /// <summary>
        /// Builds the required services and an <see cref="IGameHost"/> which hosts a game application.
        /// </summary>
        public IGameHost Build()
        {
            if (_gameHostBuilt)
                throw new InvalidOperationException("Resources.GameHostBuilder_SingleInstance");
            _gameHostBuilt = true;

            var hostingServices = BuildCommonServices(out var hostingStartupErrors);
            var applicationServices = hostingServices.Clone();
            var hostingServiceProvider = GetProviderFromFactory(hostingServices);

            if (!_options.SuppressStatusMessages)
            {
                // Warn about deprecated environment variables
                if (Environment.GetEnvironmentVariable("Hosting:Environment") != null)
                    Console.WriteLine("The environment variable 'Hosting:Environment' is obsolete and has been replaced with 'GAMENETCORE_ENVIRONMENT'");

                if (Environment.GetEnvironmentVariable("GAMENET_ENV") != null)
                    Console.WriteLine("The environment variable 'GAMENET_ENV' is obsolete and has been replaced with 'GAMENETCORE_ENVIRONMENT'");

                if (Environment.GetEnvironmentVariable("GAMENETCORE_SERVER.URLS") != null)
                    Console.WriteLine("The environment variable 'GAMENETCORE_SERVER.URLS' is obsolete and has been replaced with 'GAMENETCORE_URLS'");
            }

            AddApplicationServices(applicationServices, hostingServiceProvider);

            var host = new GameHost(
                applicationServices,
                hostingServiceProvider,
                _options,
                _config,
                hostingStartupErrors);
            try
            {
                host.Initialize();

                // resolve configuration explicitly once to mark it as resolved within the
                // service provider, ensuring it will be properly disposed with the provider
                _ = host.Services.GetService<IConfiguration>();

                var logger = host.Services.GetRequiredService<ILogger<GameHost>>();

                // Warn about duplicate HostingStartupAssemblies
                foreach (var assemblyName in _options.GetFinalHostingStartupAssemblies().GroupBy(a => a, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
                    logger.LogWarning($"The assembly {assemblyName} was specified multiple times. Hosting startup assemblies should only be specified once.");

                return host;
            }
            catch
            {
                // Dispose the host if there's a failure to initialize, this should dispose
                // services that were constructed until the exception was thrown
                host.Dispose();
                throw;
            }

            IServiceProvider GetProviderFromFactory(IServiceCollection collection)
            {
                var provider = collection.BuildServiceProvider();
                var factory = provider.GetService<IServiceProviderFactory<IServiceCollection>>();

                if (factory != null && !(factory is DefaultServiceProviderFactory))
                    using (provider)
                        return factory.CreateServiceProvider(factory.CreateBuilder(collection));

                return provider;
            }
        }

        IServiceCollection BuildCommonServices(out AggregateException hostingStartupErrors)
        {
            hostingStartupErrors = null;

            _options = new GameHostOptions(_config, Assembly.GetEntryAssembly()?.GetName().Name);

            if (!_options.PreventHostingStartup)
            {
                var exceptions = new List<Exception>();

                // Execute the hosting startup assemblies
                foreach (var assemblyName in _options.GetFinalHostingStartupAssemblies().Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    try
                    {
                        var assembly = Assembly.Load(new AssemblyName(assemblyName));

                        foreach (var attribute in assembly.GetCustomAttributes<HostingStartupAttribute>())
                        {
                            var hostingStartup = (IHostingStartup)Activator.CreateInstance(attribute.HostingStartupType);
                            hostingStartup.Configure(this);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Capture any errors that happen during startup
                        exceptions.Add(new InvalidOperationException($"Startup assembly {assemblyName} failed to execute. See the inner exception for more details.", ex));
                    }
                }

                if (exceptions.Count > 0)
                    hostingStartupErrors = new AggregateException(exceptions);
            }

            var contentRootPath = ResolveContentRootPath(_options.ContentRootPath, AppContext.BaseDirectory);

            // Initialize the hosting environment
            ((IGameHostEnvironment)_hostingEnvironment).Initialize(contentRootPath, _options);
            _context.HostingEnvironment = _hostingEnvironment;

            var services = new ServiceCollection();
            services.AddSingleton(_options);
            services.AddSingleton<IGameHostEnvironment>(_hostingEnvironment);
            services.AddSingleton<IHostEnvironment>(_hostingEnvironment);
#pragma warning disable CS0618 // Type or member is obsolete
            services.AddSingleton<GameNetCore.Hosting.IHostingEnvironment>(_hostingEnvironment);
#if !NET2
            services.AddSingleton<Extensions.Hosting.IHostingEnvironment>(_hostingEnvironment);
#endif
#pragma warning restore CS0618 // Type or member is obsolete
            services.AddSingleton(_context);

            var builder = new ConfigurationBuilder()
                .SetBasePath(_hostingEnvironment.ContentRootPath)
#if !NET2
                .AddConfiguration(_config, shouldDisposeConfiguration: true);
#else
                .AddConfiguration(_config);
#endif

            _configureAppConfigurationBuilder?.Invoke(_context, builder);

            var configuration = builder.Build();
            // register configuration as factory to make it dispose with the service provider
            services.AddSingleton<IConfiguration>(_ => configuration);
            _context.Configuration = configuration;

            var listener = new DiagnosticListener("Microsoft.GameNetCore");
            services.AddSingleton<DiagnosticListener>(listener);
            services.AddSingleton<DiagnosticSource>(listener);

            services.AddTransient<IApplicationBuilderFactory, ApplicationBuilderFactory>();
            //services.AddTransient<IProtoContextFactory, DefaultProtoContextFactory>();
            //services.AddScoped<IMiddlewareFactory, MiddlewareFactory>();
            services.AddOptions();
            services.AddLogging();

            services.AddTransient<IServiceProviderFactory<IServiceCollection>, DefaultServiceProviderFactory>();

            if (!string.IsNullOrEmpty(_options.StartupAssembly))
            {
                try
                {
                    var startupType = StartupLoader.FindStartupType(_options.StartupAssembly, _hostingEnvironment.EnvironmentName);

                    if (typeof(IStartup).GetTypeInfo().IsAssignableFrom(startupType.GetTypeInfo()))
                        services.AddSingleton(typeof(IStartup), startupType);
                    else
                    {
                        services.AddSingleton(typeof(IStartup), sp =>
                        {
                            var hostingEnvironment = sp.GetRequiredService<IHostEnvironment>();
                            var methods = StartupLoader.LoadMethods(sp, startupType, hostingEnvironment.EnvironmentName);
                            return new ConventionBasedStartup(methods);
                        });
                    }
                }
                catch (Exception ex)
                {
                    var capture = ExceptionDispatchInfo.Capture(ex);
                    services.AddSingleton<IStartup>(_ =>
                    {
                        capture.Throw();
                        return null;
                    });
                }
            }

            _configureServices?.Invoke(_context, services);

            return services;
        }

        void AddApplicationServices(IServiceCollection services, IServiceProvider hostingServiceProvider)
        {
            // We are forwarding services from hosting container so hosting container
            // can still manage their lifetime (disposal) shared instances with application services.
            // NOTE: This code overrides original services lifetime. Instances would always be singleton in
            // application container.
            var listener = hostingServiceProvider.GetService<DiagnosticListener>();
            services.Replace(ServiceDescriptor.Singleton(typeof(DiagnosticListener), listener));
            services.Replace(ServiceDescriptor.Singleton(typeof(DiagnosticSource), listener));
        }

        string ResolveContentRootPath(string contentRootPath, string basePath)
        {
            if (string.IsNullOrEmpty(contentRootPath))
                return basePath;
            if (Path.IsPathRooted(contentRootPath))
                return contentRootPath;
            return Path.Combine(Path.GetFullPath(basePath), contentRootPath);
        }
    }
}
