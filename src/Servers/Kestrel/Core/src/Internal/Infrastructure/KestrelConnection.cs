// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Contoso.GameNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Infrastructure
{
    internal class KestrelConnection
    {
        private TaskCompletionSource<object> _executionTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        public KestrelConnection(TransportConnection transportConnection)
        {
            TransportConnection = transportConnection;
            ExecutionTask = _executionTcs.Task;
        }

        public TransportConnection TransportConnection { get; }

        public Task ExecutionTask { get; }

        internal void Complete() => _executionTcs.TrySetResult(null);
    }
}
