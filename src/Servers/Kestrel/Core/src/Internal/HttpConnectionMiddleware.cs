// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Contoso.GameNetCore.Connections;
using Contoso.GameNetCore.Connections.Features;
using Contoso.GameNetCore.Hosting.Server;
using Contoso.GameNetCore.Proto.Features;
using Contoso.GameNetCore.Server.Kestrel.Core.Adapter.Internal;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal
{
    internal class ProtoConnectionMiddleware<TContext>
    {
        private readonly IList<IConnectionAdapter> _connectionAdapters;
        private readonly ServiceContext _serviceContext;
        private readonly IProtoApplication<TContext> _application;
        private readonly ProtoProtocols _protocols;

        public ProtoConnectionMiddleware(IList<IConnectionAdapter> adapters, ServiceContext serviceContext, IProtoApplication<TContext> application, ProtoProtocols protocols)
        {
            _serviceContext = serviceContext;
            _application = application;
            _protocols = protocols;

            // Keeping these around for now so progress can be made without updating tests
            _connectionAdapters = adapters;
        }

        public Task OnConnectionAsync(ConnectionContext connectionContext)
        {
            // We need the transport feature so that we can cancel the output reader that the transport is using
            // This is a bit of a hack but it preserves the existing semantics
            var memoryPoolFeature = connectionContext.Features.Get<IMemoryPoolFeature>();

            var httpConnectionContext = new ProtoConnectionContext
            {
                ConnectionId = connectionContext.ConnectionId,
                ConnectionContext = connectionContext,
                Protocols = _protocols,
                ServiceContext = _serviceContext,
                ConnectionFeatures = connectionContext.Features,
                MemoryPool = memoryPoolFeature.MemoryPool,
                ConnectionAdapters = _connectionAdapters,
                Transport = connectionContext.Transport
            };

            var connectionFeature = connectionContext.Features.Get<IProtoConnectionFeature>();

            if (connectionFeature != null)
            {
                if (connectionFeature.LocalIpAddress != null)
                {
                    httpConnectionContext.LocalEndPoint = new IPEndPoint(connectionFeature.LocalIpAddress, connectionFeature.LocalPort);
                }

                if (connectionFeature.RemoteIpAddress != null)
                {
                    httpConnectionContext.RemoteEndPoint = new IPEndPoint(connectionFeature.RemoteIpAddress, connectionFeature.RemotePort);
                }
            }

            var connection = new ProtoConnection(httpConnectionContext);

            return connection.ProcessRequestsAsync(_application);
        }
    }
}
