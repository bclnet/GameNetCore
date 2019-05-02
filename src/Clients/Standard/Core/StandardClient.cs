// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Hosting.Client;
using Contoso.GameNetCore.Hosting.Client.Features;
using Contoso.GameNetCore.Client.Standard.Core.Internal;
using Contoso.GameNetCore.Client.Standard.Core.Internal.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.Client.Standard.Core
{
    public class StandardClient : IClient
    {
        readonly IClientAddressesFeature _clientAddresses;

        bool _hasStarted;
        int _stopping;
        readonly TaskCompletionSource<object> _stoppedTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

#pragma warning disable PUB0001 // Pubternal type in public API
        public StandardClient(IOptions<StandardClientOptions> options, ILoggerFactory loggerFactory)
#pragma warning restore PUB0001
            : this(CreateServiceContext(options, loggerFactory)) { }

        // For testing
        internal StandardClient(ServiceContext serviceContext)
        {
            ServiceContext = serviceContext;

            Features = new FeatureCollection();
            _clientAddresses = new ClientAddressesFeature();
            Features.Set(_clientAddresses);
        }

        static ServiceContext CreateServiceContext(IOptions<StandardClientOptions> options, ILoggerFactory loggerFactory)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
            var clientOptions = options.Value ?? new StandardClientOptions();
            var logger = loggerFactory.CreateLogger("Contoso.GameNetCore.Client.Standard");
            var trace = new StandardTrace(logger);
            return new ServiceContext
            {
                Log = trace,
                ClientOptions = clientOptions,
            };
        }

        public IFeatureCollection Features { get; }

        public StandardClientOptions Options => ServiceContext.ClientOptions;

        ServiceContext ServiceContext { get; }

        IStandardTrace Trace => ServiceContext.Log;

        public async Task StartAsync<TContext>(IProtoApplication<TContext> application, CancellationToken cancellationToken)
        {
            try
            {
                if (!BitConverter.IsLittleEndian)
                    throw new PlatformNotSupportedException("CoreStrings.BigEndianNotSupported");
                ValidateOptions();

                if (_hasStarted)
                    // The server has already started and/or has not been cleaned up yet
                    throw new InvalidOperationException("CoreStrings.ClientAlreadyStarted");
                _hasStarted = true;
            }
            catch (Exception ex)
            {
                Trace.LogCritical(0, ex, "Unable to start Standard.");
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
