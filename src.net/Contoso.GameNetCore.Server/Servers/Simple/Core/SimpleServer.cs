// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Hosting.Server;
using Contoso.GameNetCore.Hosting.Server.Features;
using Contoso.GameNetCore.Server.Simple.Core.Internal;
using Contoso.GameNetCore.Server.Simple.Core.Internal.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.Server.Simple.Core
{
    public class SimpleServer : IServer
    {
        readonly IServerAddressesFeature _serverAddresses;

        bool _hasStarted;
        int _stopping;
        readonly TaskCompletionSource<object> _stoppedTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

#pragma warning disable PUB0001 // Pubternal type in public API
        public SimpleServer(IOptions<SimpleServerOptions> options, ILoggerFactory loggerFactory)
#pragma warning restore PUB0001
            : this(CreateServiceContext(options, loggerFactory)) { }

        // For testing
        internal SimpleServer(ServiceContext serviceContext)
        {
            ServiceContext = serviceContext;

            Features = new FeatureCollection();
            _serverAddresses = new ServerAddressesFeature();
            Features.Set(_serverAddresses);
        }

        static ServiceContext CreateServiceContext(IOptions<SimpleServerOptions> options, ILoggerFactory loggerFactory)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
            var serverOptions = options.Value ?? new SimpleServerOptions();
            var logger = loggerFactory.CreateLogger("Contoso.GameNetCore.Server.Simple");
            var trace = new SimpleTrace(logger);
            return new ServiceContext
            {
                Log = trace,
                ServerOptions = serverOptions,
            };
        }

        public IFeatureCollection Features { get; }

        public SimpleServerOptions Options => ServiceContext.ServerOptions;

        ServiceContext ServiceContext { get; }

        ISimpleTrace Trace => ServiceContext.Log;

        public async Task StartAsync<TContext>(IProtoApplication<TContext> application, CancellationToken cancellationToken)
        {
            try
            {
                if (!BitConverter.IsLittleEndian)
                    throw new PlatformNotSupportedException("CoreStrings.BigEndianNotSupported");
                ValidateOptions();

                if (_hasStarted)
                    // The server has already started and/or has not been cleaned up yet
                    throw new InvalidOperationException("CoreStrings.ServerAlreadyStarted");
                _hasStarted = true;
            }
            catch (Exception ex)
            {
                Trace.LogCritical(0, ex, "Unable to start Simple.");
                Dispose();
                throw;
            }
        }

        // Graceful shutdown if possible
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.Exchange(ref _stopping, 1) == 1)
            {
                await _stoppedTcs.Task.ConfigureAwait(false);
                return;
            }

            _stoppedTcs.TrySetResult(null);
        }

        // Ungraceful shutdown
        public void Dispose()
        {
            var cancelledTokenSource = new CancellationTokenSource();
            cancelledTokenSource.Cancel();
            StopAsync(cancelledTokenSource.Token).GetAwaiter().GetResult();
        }

        void ValidateOptions()
        {
            Options.ConfigurationLoader?.Load();
        }
    }
}
