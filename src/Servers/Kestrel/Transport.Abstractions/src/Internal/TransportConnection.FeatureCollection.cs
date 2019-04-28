// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using Contoso.GameNetCore.Connections;
using Contoso.GameNetCore.Connections.Features;
using Contoso.GameNetCore.Proto.Features;

namespace Contoso.GameNetCore.Server.Kestrel.Transport.Abstractions.Internal
{
    public partial class TransportConnection : IProtoConnectionFeature,
                                               IConnectionIdFeature,
                                               IConnectionTransportFeature,
                                               IConnectionItemsFeature,
                                               IMemoryPoolFeature,
                                               IApplicationTransportFeature,
                                               ITransportSchedulerFeature,
                                               IConnectionLifetimeFeature,
                                               IConnectionHeartbeatFeature,
                                               IConnectionLifetimeNotificationFeature
    {
        // NOTE: When feature interfaces are added to or removed from this TransportConnection class implementation,
        // then the list of `features` in the generated code project MUST also be updated.
        // See also: tools/CodeGenerator/TransportConnectionFeatureCollection.cs

        string IProtoConnectionFeature.ConnectionId
        {
            get => ConnectionId;
            set => ConnectionId = value;
        }

        IPAddress IProtoConnectionFeature.RemoteIpAddress
        {
            get => RemoteAddress;
            set => RemoteAddress = value;
        }

        IPAddress IProtoConnectionFeature.LocalIpAddress
        {
            get => LocalAddress;
            set => LocalAddress = value;
        }

        int IProtoConnectionFeature.RemotePort
        {
            get => RemotePort;
            set => RemotePort = value;
        }

        int IProtoConnectionFeature.LocalPort
        {
            get => LocalPort;
            set => LocalPort = value;
        }

        MemoryPool<byte> IMemoryPoolFeature.MemoryPool => MemoryPool;

        IDuplexPipe IConnectionTransportFeature.Transport
        {
            get => Transport;
            set => Transport = value;
        }

        IDuplexPipe IApplicationTransportFeature.Application
        {
            get => Application;
            set => Application = value;
        }

        IDictionary<object, object> IConnectionItemsFeature.Items
        {
            get => Items;
            set => Items = value;
        }

        PipeScheduler ITransportSchedulerFeature.InputWriterScheduler => InputWriterScheduler;
        PipeScheduler ITransportSchedulerFeature.OutputReaderScheduler => OutputReaderScheduler;

        CancellationToken IConnectionLifetimeFeature.ConnectionClosed
        {
            get => ConnectionClosed;
            set => ConnectionClosed = value;
        }

        CancellationToken IConnectionLifetimeNotificationFeature.ConnectionClosedRequested
        {
            get => ConnectionClosedRequested;
            set => ConnectionClosedRequested = value;
        }

        void IConnectionLifetimeFeature.Abort() => Abort(new ConnectionAbortedException("The connection was aborted by the application via IConnectionLifetimeFeature.Abort()."));

        void IConnectionLifetimeNotificationFeature.RequestClose() => RequestClose();

        void IConnectionHeartbeatFeature.OnHeartbeat(System.Action<object> action, object state)
        {
            OnHeartbeat(action, state);
        }
    }
}
