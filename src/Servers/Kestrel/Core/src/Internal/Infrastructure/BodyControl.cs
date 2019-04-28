// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Pipelines;
using Contoso.GameNetCore.Proto.Features;
using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Infrastructure
{
    internal class BodyControl
    {
        private static readonly ThrowingWasUpgradedWriteOnlyStream _throwingResponseStream
            = new ThrowingWasUpgradedWriteOnlyStream();
        private readonly ProtoResponseStream _response;
        private readonly ProtoResponsePipeWriter _responseWriter;
        private readonly ProtoRequestPipeReader _requestReader;
        private readonly ProtoRequestStream _request;
        private readonly ProtoRequestPipeReader _emptyRequestReader;
        private readonly WrappingStream _upgradeableResponse;
        private readonly ProtoRequestStream _emptyRequest;
        private readonly Stream _upgradeStream;

        public BodyControl(IProtoBodyControlFeature bodyControl, IProtoResponseControl responseControl)
        {
            _requestReader = new ProtoRequestPipeReader();
            _request = new ProtoRequestStream(bodyControl, _requestReader);
            _emptyRequestReader = new ProtoRequestPipeReader();
            _emptyRequest = new ProtoRequestStream(bodyControl, _emptyRequestReader);

            _responseWriter = new ProtoResponsePipeWriter(responseControl);
            _response = new ProtoResponseStream(bodyControl, _responseWriter);
            _upgradeableResponse = new WrappingStream(_response);
            _upgradeStream = new ProtoUpgradeStream(_request, _response);
        }

        public Stream Upgrade()
        {
            // causes writes to context.Response.Body to throw
            _upgradeableResponse.SetInnerStream(_throwingResponseStream);
            // _upgradeStream always uses _response
            return _upgradeStream;
        }

        public (Stream request, Stream response, PipeReader reader, PipeWriter writer) Start(MessageBody body)
        {
            _requestReader.StartAcceptingReads(body);
            _emptyRequestReader.StartAcceptingReads(MessageBody.ZeroContentLengthClose);
            _responseWriter.StartAcceptingWrites();

            if (body.RequestUpgrade)
            {
                // until Upgrade() is called, context.Response.Body should use the normal output stream
                _upgradeableResponse.SetInnerStream(_response);
                // upgradeable requests should never have a request body
                return (_emptyRequest, _upgradeableResponse, _emptyRequestReader, _responseWriter);
            }
            else
            {
                return (_request, _response, _requestReader, _responseWriter);
            }
        }

        public void Stop()
        {
            _requestReader.StopAcceptingReads();
            _emptyRequestReader.StopAcceptingReads();
            _responseWriter.StopAcceptingWrites();
        }

        public void Abort(Exception error)
        {
            _requestReader.Abort(error);
            _emptyRequestReader.Abort(error);
            _responseWriter.Abort();
        }
    }
}
