// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Connections;
using Contoso.GameNetCore.Server.Kestrel.Core.Adapter.Internal;
using Contoso.GameNetCore.Testing;

namespace Contoso.GameNetCore.Server.Kestrel.InMemory.FunctionalTests.TestTransport
{
    internal class InMemoryConnection : StreamBackedTestConnection
    {
        public InMemoryConnection(InMemoryTransportConnection transportConnection)
            : base(new RawStream(transportConnection.Output, transportConnection.Input))
        {
            TransportConnection = transportConnection;
        }

        public InMemoryTransportConnection TransportConnection { get; }

        public override void Reset()
        {
            TransportConnection.Input.Complete(new ConnectionResetException(string.Empty));
            TransportConnection.OnClosed();
        }

        public override void ShutdownSend()
        {
            TransportConnection.Input.Complete();
            TransportConnection.OnClosed();
        }

        public override void Dispose()
        {
            TransportConnection.Input.Complete();
            TransportConnection.Output.Complete();
            TransportConnection.OnClosed();
            base.Dispose();
        }
    }
}
