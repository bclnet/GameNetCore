// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Connections;
using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2;
using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2.HPack;
using Microsoft.Extensions.Logging;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Infrastructure
{
    internal interface IKestrelTrace : ILogger
    {
        void ConnectionStart(string connectionId);

        void ConnectionStop(string connectionId);

        void ConnectionPause(string connectionId);

        void ConnectionResume(string connectionId);

        void ConnectionRejected(string connectionId);

        void ConnectionKeepAlive(string connectionId);

        void ConnectionDisconnect(string connectionId);

        void RequestProcessingError(string connectionId, Exception ex);

        void ConnectionHeadResponseBodyWrite(string connectionId, long count);

        void NotAllConnectionsClosedGracefully();

        void ConnectionBadRequest(string connectionId, BadProtoRequestException ex);

        void ApplicationError(string connectionId, string traceIdentifier, Exception ex);

        void NotAllConnectionsAborted();

        void HeartbeatSlow(TimeSpan interval, DateTimeOffset now);

        void ApplicationNeverCompleted(string connectionId);

        void RequestBodyStart(string connectionId, string traceIdentifier);

        void RequestBodyDone(string connectionId, string traceIdentifier);

        void RequestBodyNotEntirelyRead(string connectionId, string traceIdentifier);

        void RequestBodyDrainTimedOut(string connectionId, string traceIdentifier);

        void RequestBodyMinimumDataRateNotSatisfied(string connectionId, string traceIdentifier, double rate);

        void ResponseMinimumDataRateNotSatisfied(string connectionId, string traceIdentifier);

        void ApplicationAbortedConnection(string connectionId, string traceIdentifier);

        void Proto2ConnectionError(string connectionId, Proto2ConnectionErrorException ex);

        void Proto2ConnectionClosing(string connectionId);

        void Proto2ConnectionClosed(string connectionId, int highestOpenedStreamId);

        void Proto2StreamError(string connectionId, Proto2StreamErrorException ex);

        void Proto2StreamResetAbort(string traceIdentifier, Proto2ErrorCode error, ConnectionAbortedException abortReason);

        void HPackDecodingError(string connectionId, int streamId, HPackDecodingException ex);

        void HPackEncodingError(string connectionId, int streamId, HPackEncodingException ex);

        void Proto2FrameReceived(string connectionId, Proto2Frame frame);

        void Proto2FrameSending(string connectionId, Proto2Frame frame);
    }
}
