// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto
{
    /// <summary>
    /// Default ProtoRequest PipeReader implementation to be used by Kestrel.
    /// </summary>
    internal class ProtoRequestPipeReader : PipeReader
    {
        private MessageBody _body;
        private ProtoStreamState _state;
        private Exception _error;

        public ProtoRequestPipeReader()
        {
            _state = ProtoStreamState.Closed;
        }

        public override void AdvanceTo(SequencePosition consumed)
        {
            ValidateState();

            _body.AdvanceTo(consumed);
        }

        public override void AdvanceTo(SequencePosition consumed, SequencePosition examined)
        {
            ValidateState();

            _body.AdvanceTo(consumed, examined);
        }

        public override void CancelPendingRead()
        {
            ValidateState();

            _body.CancelPendingRead();
        }

        public override void Complete(Exception exception = null)
        {
            ValidateState();

            _body.Complete(exception);
        }

        public override void OnWriterCompleted(Action<Exception, object> callback, object state)
        {
            ValidateState();

            _body.OnWriterCompleted(callback, state);
        }

        public override ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            ValidateState(cancellationToken);

            return _body.ReadAsync(cancellationToken);
        }

        public override bool TryRead(out ReadResult result)
        {
            ValidateState();

            return _body.TryRead(out result);
        }

        public void StartAcceptingReads(MessageBody body)
        {
            // Only start if not aborted
            if (_state == ProtoStreamState.Closed)
            {
                _state = ProtoStreamState.Open;
                _body = body;
            }
        }

        public void StopAcceptingReads()
        {
            // Can't use dispose (or close) as can be disposed too early by user code
            // As exampled in EngineTests.ZeroContentLengthNotSetAutomaticallyForCertainStatusCodes
            _state = ProtoStreamState.Closed;
            _body = null;
        }

        public void Abort(Exception error = null)
        {
            // We don't want to throw an ODE until the app func actually completes.
            // If the request is aborted, we throw a TaskCanceledException instead,
            // unless error is not null, in which case we throw it.
            if (_state != ProtoStreamState.Closed)
            {
                _state = ProtoStreamState.Aborted;
                _error = error;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateState(CancellationToken cancellationToken = default)
        {
            var state = _state;
            if (state == ProtoStreamState.Open)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            else if (state == ProtoStreamState.Closed)
            {
                ThrowObjectDisposedException();
            }
            else
            {
                if (_error != null)
                {
                    ExceptionDispatchInfo.Capture(_error).Throw();
                }
                else
                {
                    ThrowTaskCanceledException();
                }
            }

            void ThrowObjectDisposedException() => throw new ObjectDisposedException(nameof(ProtoRequestStream));
            void ThrowTaskCanceledException() => throw new TaskCanceledException();
        }
    }
}
