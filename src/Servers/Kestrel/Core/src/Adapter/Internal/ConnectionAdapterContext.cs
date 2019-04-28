// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Contoso.GameNetCore.Connections;
using Contoso.GameNetCore.Proto.Features;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Adapter.Internal
{
    // Even though this only includes the non-adapted ConnectionStream currently, this is a context in case
    // we want to add more connection metadata later.
    public class ConnectionAdapterContext
    {
        internal ConnectionAdapterContext(ConnectionContext connectionContext, Stream connectionStream)
        {
            ConnectionContext = connectionContext;
            ConnectionStream = connectionStream;
        }

        internal ConnectionContext ConnectionContext { get; }

        public IFeatureCollection Features => ConnectionContext.Features;

        public Stream ConnectionStream { get; }
    }
}
