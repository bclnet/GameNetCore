// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Hosting.Builder;
using Contoso.GameNetCore.Hosting.Server;
using Contoso.GameNetCore.Hosting.Server.Features;
using Contoso.GameNetCore.Hosting.Views;
#if NETX
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using IProtoContextFactory = Microsoft.AspNetCore.Http.IHttpContextFactory;
#else
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Proto;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.Hosting.Internal
{
    internal class GenericGameHostService : IHostedService
    {
        public GenericGameHostService(IOptions<GenericGameHostServiceOptions> options,
                                     IServer server,
                                     ILoggerFactory loggerFactory,
                                     DiagnosticListener diagnosticListener,
                                     IProtoContextFactory protoContextFactory,
                                     IApplicationBuilderFactory applicationBuilderFactory,
                                     IEnumerable<IStartupFilter> startupFilters,
                                     IConfiguration configuration,
                                     IGameHostEnvironment hostingEnvironment)
        {
            Options = options.Value;
            Server = server;
            Logger = loggerFactory.CreateLogger<GenericGameHostService>();
            LifetimeLogger = loggerFactory.CreateLogger("Microsoft.Hosting.Lifetime");
            DiagnosticListener = diagnosticListener;
            ProtoContextFactory = protoContextFactory;
            ApplicationBuilderFactory = applicationBuilderFactory;
            StartupFilters = startupFilters;
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public GenericGameHostServiceOptions Options { get; }
        public IServer Server { get; }
        public ILogger<GenericGameHostService> Logger { get; }
        // Only for high level lifetime events
        public ILogger LifetimeLogger { get; }
        public DiagnosticListener DiagnosticListener { get; }
        public IProtoContextFactory ProtoContextFactory { get; }
        public IApplicationBuilderFactory ApplicationBuilderFactory { get; }
        public IEnumerable<IStartupFilter> StartupFilters { get; }
        public IConfiguration Configuration { get; }
        public IGameHostEnvironment HostingEnvironment { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            HostingEventSource.Log.HostStart();

            var serverAddressesFeature = Server.Features?.Get<IServerAddressesFeature>();
            var addresses = serverAddressesFeature?.Addresses;
            if (addresses != null && !addresses.IsReadOnly && addresses.Count == 0)
            {
                var urls = Configuration[GameHostDefaults.ServerUrlsKey];
                if (!string.IsNullOrEmpty(urls))
                {
                    serverAddressesFeature.PreferHostingUrls = GameHostUtilities.ParseBool(Configuration, GameHostDefaults.PreferHostingUrlsKey);

                    foreach (var value in urls.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        addresses.Add(value);
                }
            }

            RequestDelegate application = null;

            try
            {
                Action<IApplicationBuilder> configure = Options.ConfigureApplication;

                if (configure == null)
                    throw new InvalidOperationException($"No application configured. Please specify an application via IGameHostBuilder.UseStartup, IGameHostBuilder.Configure, or specifying the startup assembly via {nameof(GameHostDefaults.StartupAssemblyKey)} in the game host configuration.");

                var builder = ApplicationBuilderFactory.CreateBuilder(Server.Features);

                foreach (var filter in StartupFilters.Reverse())
                    configure = filter.Configure(configure);

                configure(builder);

                // Build the request pipeline
                application = builder.Build();
            }
            catch (Exception ex)
            {
                Logger.ApplicationError(ex);

                if (!Options.GameHostOptions.CaptureStartupErrors)
                    throw;

                application = BuildErrorPageApplication(ex);
            }

            var httpApplication = new HostingApplication(application, Logger, DiagnosticListener, ProtoContextFactory);

            await Server.StartAsync(httpApplication, cancellationToken);

            if (addresses != null)
                foreach (var address in addresses)
                    LifetimeLogger.LogInformation("Now listening on: {address}", address);

            if (Logger.IsEnabled(LogLevel.Debug))
                foreach (var assembly in Options.GameHostOptions.GetFinalHostingStartupAssemblies())
                    Logger.LogDebug("Loaded hosting startup assembly {assemblyName}", assembly);

            if (Options.HostingStartupExceptions != null)
                foreach (var exception in Options.HostingStartupExceptions.InnerExceptions)
                    Logger.HostingStartupAssemblyError(exception);
        }

        RequestDelegate BuildErrorPageApplication(Exception exception)
        {
            if (exception is TargetInvocationException tae)
                exception = tae.InnerException;

            var showDetailedErrors = HostingEnvironment.IsDevelopment() || Options.GameHostOptions.DetailedErrors;

            var model = new ErrorPageModel
            {
                RuntimeDisplayName = RuntimeInformation.FrameworkDescription
            };
            var systemRuntimeAssembly = typeof(System.ComponentModel.DefaultValueAttribute).GetTypeInfo().Assembly;
            var assemblyVersion = new AssemblyName(systemRuntimeAssembly.FullName).Version.ToString();
            var clrVersion = assemblyVersion;
            model.RuntimeArchitecture = RuntimeInformation.ProcessArchitecture.ToString();
            var currentAssembly = typeof(ErrorPage).GetTypeInfo().Assembly;
            model.CurrentAssemblyVesion = currentAssembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;
            model.ClrVersion = clrVersion;
            model.OperatingSystemDescription = RuntimeInformation.OSDescription;

            //if (showDetailedErrors)
            //{
            //    var exceptionDetailProvider = new ExceptionDetailsProvider(
            //        HostingEnvironment.ContentRootFileProvider,
            //        sourceCodeLineCount: 6);

            //    model.ErrorDetails = exceptionDetailProvider.GetDetails(exception);
            //}
            //else
            //    model.ErrorDetails = new ExceptionDetails[0];

            var errorPage = new ErrorPage(model);
            return context =>
            {
                context.Response.StatusCode = 500;
                context.Response.Headers["Cache-Control"] = "no-cache";
                return errorPage.ExecuteAsync(context);
            };
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try { await Server.StopAsync(cancellationToken); }
            finally
            {
                HostingEventSource.Log.HostStop();
            }
        }
    }
}
