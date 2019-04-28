// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto
{
    internal class ProtoResponsePipeWriter : PipeWriter
    {
        private ProtoStreamState _state;
        private readonly IProtoResponseControl _pipeControl;

        public ProtoResponsePipeWriter(IProtoResponseControl pipeControl)
        {
            _pipeControl = pipeControl;
            _state = ProtoStreamState.Closed;
        }

        public override void Advance(int bytes)
        {
            ValidateState();
            _pipeControl.Advance(bytes);
        }

        public override void CancelPendingFlush()
        {
            ValidateState();
            _pipeControl.CancelPendingFlush();
        }

        public override void Complete(Exception exception = null)
        {
            ValidateState();
            _pipeControl.Complete(exception);
        }

        public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
        {
            ValidateState(cancellationToken);
            return _pipeControl.FlushPipeAsync(cancellationToken);
        }

        public override Memory<byte> GetMemory(int sizeHint = 0)
        {
            ValidateState();
            return _pipeControl.GetMemory(sizeHint);
        }

        public override Span<byte> GetSpan(int sizeHint = 0)
        {
            ValidateState();
            return _pipeControl.GetSpan(sizeHint);
        }

        public override void OnReaderCompleted(Action<Exception, object> callback, object state)
        {
            ValidateState();
            throw new NotSupportedException();
        }

        public override ValueTask<FlushResult> WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
        {
            ValidateState(cancellationToken);
            return _pipeControl.WritePipeAsync(source, cancellationToken);
        }

        public void StartAcceptingWrites()
        {
            // Only start if not aborted
            if (_state == ProtoStreamState.Closed)
            {
                _state = ProtoStreamState.Open;
            }
        }

        public void StopAcceptingWrites()
        {
            // Can't use dispose (or close) as can be disposed too early by user code
            // As exampled in EngineTests.ZeroContentLengthNotSetAutomaticallyForCertainStatusCodes
            _state = ProtoStreamState.Closed;
        }

        public void Abort()
        {
            // We don't want to throw an ODE until the app func actually completes.
            if (_state != ProtoStreamState.Closed)
            {
                _state = ProtoStreamState.Aborted;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateState(CancellationToken cancellationToken = default)
        {
            var state = _state;
            if (state == ProtoStreamState.Open || state == ProtoStreamState.Aborted)
            {
                // Aborted state only throws on write if cancellationToken requests it
                cancellationToken.ThrowIfCancellationRequested();
            }
            else
            {
                ThrowObjectDisposedException();
            }

            void ThrowObjectDisposedException()
            {
                throw new ObjectDisposedException(nameof(ProtoResponseStream), CoreStrings.WritingToResponseBodyAfterResponseCompleted);
            }
        }
    }
}
