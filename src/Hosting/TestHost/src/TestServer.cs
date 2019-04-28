// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Proto;
using System.Threading;
using System.Threading.Tasks;
using Contoso.GameNetCore.Hosting;
using Contoso.GameNetCore.Hosting.Internal;
using Contoso.GameNetCore.Hosting.Server;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Proto.Features;
using Context = Microsoft.GameNetCore.Hosting.Internal.HostingApplication.Context;

namespace Contoso.GameNetCore.TestHost
{
    public class TestServer : IServer
    {
        private IGameHost _hostInstance;
        private bool _disposed = false;
        private IProtoApplication<Context> _application;

        /// <summary>
        /// For use with IHostBuilder or IGameHostBuilder.
        /// </summary>
        public TestServer()
            : this(new FeatureCollection())
        {
        }

        /// <summary>
        /// For use with IHostBuilder or IGameHostBuilder.
        /// </summary>
        /// <param name="featureCollection"></param>
        public TestServer(IFeatureCollection featureCollection)
        {
            Features = featureCollection ?? throw new ArgumentNullException(nameof(featureCollection));
        }

        /// <summary>
        /// For use with IGameHostBuilder.
        /// </summary>
        /// <param name="builder"></param>
        public TestServer(IGameHostBuilder builder)
            : this(builder, new FeatureCollection())
        {
        }

        /// <summary>
        /// For use with IGameHostBuilder.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="featureCollection"></param>
        public TestServer(IGameHostBuilder builder, IFeatureCollection featureCollection)
            : this(featureCollection)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var host = builder.UseServer(this).Build();
            host.StartAsync().GetAwaiter().GetResult();
            _hostInstance = host;
        }

        public Uri BaseAddress { get; set; } = new Uri("http://localhost/");

        public IGameHost Host
        {
            get
            {
                return _hostInstance
                    ?? throw new InvalidOperationException("The TestServer constructor was not called with a IGameHostBuilder so IGameHost is not available.");
            }
        }

        public IFeatureCollection Features { get; }

        /// <summary>
        /// Gets or sets a value that controls whether synchronous IO is allowed for the <see cref="ProtoContext.Request"/> and <see cref="ProtoContext.Response"/>
        /// </summary>
        /// <remarks>
        /// Defaults to false.
        /// </remarks>
        public bool AllowSynchronousIO { get; set; } = false;

        private IProtoApplication<Context> Application
        {
            get => _application ?? throw new InvalidOperationException("The server has not been started or no game application was configured.");
        }

        public ProtoMessageHandler CreateHandler()
        {
            var pathBase = BaseAddress == null ? PathString.Empty : PathString.FromUriComponent(BaseAddress);
            return new ClientHandler(pathBase, Application) { AllowSynchronousIO = AllowSynchronousIO };
        }

        public ProtoClient CreateClient()
        {
            return new ProtoClient(CreateHandler()) { BaseAddress = BaseAddress };
        }

        public GameSocketClient CreateGameSocketClient()
        {
            var pathBase = BaseAddress == null ? PathString.Empty : PathString.FromUriComponent(BaseAddress);
            return new GameSocketClient(pathBase, Application) { AllowSynchronousIO = AllowSynchronousIO };
        }

        /// <summary>
        /// Begins constructing a request message for submission.
        /// </summary>
        /// <param name="path"></param>
        /// <returns><see cref="RequestBuilder"/> to use in constructing additional request details.</returns>
        public RequestBuilder CreateRequest(string path)
        {
            return new RequestBuilder(this, path);
        }

        /// <summary>
        /// Creates, configures, sends, and returns a <see cref="ProtoContext"/>. This completes as soon as the response is started.
        /// </summary>
        /// <returns></returns>
        public async Task<ProtoContext> SendAsync(Action<ProtoContext> configureContext, CancellationToken cancellationToken = default)
        {
            if (configureContext == null)
            {
                throw new ArgumentNullException(nameof(configureContext));
            }

            var builder = new ProtoContextBuilder(Application, AllowSynchronousIO);
            builder.Configure(context =>
            {
                var request = context.Request;
                request.Scheme = BaseAddress.Scheme;
                request.Host = HostString.FromUriComponent(BaseAddress);
                if (BaseAddress.IsDefaultPort)
                {
                    request.Host = new HostString(request.Host.Host);
                }
                var pathBase = PathString.FromUriComponent(BaseAddress);
                if (pathBase.HasValue && pathBase.Value.EndsWith("/"))
                {
                    pathBase = new PathString(pathBase.Value.Substring(0, pathBase.Value.Length - 1));
                }
                request.PathBase = pathBase;
            });
            builder.Configure(configureContext);
            // TODO: Wrap the request body if any?
            return await builder.SendAsync(cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _hostInstance?.Dispose();
            }
        }

        Task IServer.StartAsync<TContext>(IProtoApplication<TContext> application, CancellationToken cancellationToken)
        {
            _application = new ApplicationWrapper<Context>((IProtoApplication<Context>)application, () =>
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
            });

            return Task.CompletedTask;
        }

        Task IServer.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private class ApplicationWrapper<TContext> : IProtoApplication<TContext>
        {
            private readonly IProtoApplication<TContext> _application;
            private readonly Action _preProcessRequestAsync;

            public ApplicationWrapper(IProtoApplication<TContext> application, Action preProcessRequestAsync)
            {
                _application = application;
                _preProcessRequestAsync = preProcessRequestAsync;
            }

            public TContext CreateContext(IFeatureCollection contextFeatures)
            {
                return _application.CreateContext(contextFeatures);
            }

            public void DisposeContext(TContext context, Exception exception)
            {
                _application.DisposeContext(context, exception);
            }

            public Task ProcessRequestAsync(TContext context)
            {
                _preProcessRequestAsync();
                return _application.ProcessRequestAsync(context);
            }
        }
    }
}
