// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using Contoso.GameNetCore.Connections;
using Contoso.GameNetCore.Proto.Features;
using Contoso.GameNetCore.Server.Kestrel.Core.Adapter.Internal;
using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Infrastructure;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal
{
    internal class ProtoConnectionContext
    {
        public string ConnectionId { get; set; }
        public ProtoProtocols Protocols { get; set; }
        public ConnectionContext ConnectionContext { get; set; }
        public ServiceContext ServiceContext { get; set; }
        public IFeatureCollection ConnectionFeatures { get; set; }
        public IList<IConnectionAdapter> ConnectionAdapters { get; set; }
        public MemoryPool<byte> MemoryPool { get; set; }
        public IPEndPoint LocalEndPoint { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }
        public ITimeoutControl TimeoutControl { get; set; }
        public IDuplexPipe Transport { get; set; }
    }
}
