// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Contoso.GameNetCore.Connections;
using Contoso.GameNetCore.Proto.Features;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto
{
    internal class ProtoRequestStream : ReadOnlyPipeStream
    {
        private ProtoRequestPipeReader _pipeReader;
        private readonly IProtoBodyControlFeature _bodyControl;

        public ProtoRequestStream(IProtoBodyControlFeature bodyControl, ProtoRequestPipeReader pipeReader)
            : base (pipeReader)
        {
            _bodyControl = bodyControl;
            _pipeReader = pipeReader;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return ReadAsyncInternal(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();
        }

        public override ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
        {
            return ReadAsyncInternal(destination, cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!_bodyControl.AllowSynchronousIO)
            {
                throw new InvalidOperationException(CoreStrings.SynchronousReadsDisallowed);
            }

            return ReadAsync(buffer, offset, count).GetAwaiter().GetResult();
        }

        private ValueTask<int> ReadAsyncInternal(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            try
            {
                return base.ReadAsync(buffer, cancellationToken);
            }
            catch (ConnectionAbortedException ex)
            {
                throw new TaskCanceledException("The request was aborted", ex);
            }
        }

        public override void Flush()
        {
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
