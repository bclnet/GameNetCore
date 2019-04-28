// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Contoso.GameNetCore.Connections;
using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Infrastructure;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto
{
    internal abstract class Proto1MessageBody : MessageBody
    {
        protected readonly Proto1Connection _context;

        protected Proto1MessageBody(Proto1Connection context)
            : base(context, context.MinRequestBodyDataRate)
        {
            _context = context;
        }

        protected void CheckCompletedReadResult(ReadResult result)
        {
            if (result.IsCompleted)
            {
                // OnInputOrOutputCompleted() is an idempotent method that closes the connection. Sometimes
                // input completion is observed here before the Input.OnWriterCompleted() callback is fired,
                // so we call OnInputOrOutputCompleted() now to prevent a race in our tests where a 400
                // response is written after observing the unexpected end of request content instead of just
                // closing the connection without a response as expected.
                _context.OnInputOrOutputCompleted();

                BadProtoRequestException.Throw(RequestRejectionReason.UnexpectedEndOfRequestContent);
            }
        }

        protected override Task OnConsumeAsync()
        {
            try
            {
                if (TryRead(out var readResult))
                {
                    AdvanceTo(readResult.Buffer.End);

                    if (readResult.IsCompleted)
                    {
                        return Task.CompletedTask;
                    }
                }
            }
            catch (BadProtoRequestException ex)
            {
                // At this point, the response has already been written, so this won't result in a 4XX response;
                // however, we still need to stop the request processing loop and log.
                _context.SetBadRequestState(ex);
                return Task.CompletedTask;
            }
            catch (InvalidOperationException ex)
            {
                var connectionAbortedException = new ConnectionAbortedException(CoreStrings.ConnectionAbortedByApplication, ex);
                _context.ReportApplicationError(connectionAbortedException);

                // Have to abort the connection because we can't finish draining the request
                _context.StopProcessingNextRequest();
                return Task.CompletedTask;
            }

            return OnConsumeAsyncAwaited();
        }

        protected async Task OnConsumeAsyncAwaited()
        {
            Log.RequestBodyNotEntirelyRead(_context.ConnectionIdFeature, _context.TraceIdentifier);

            _context.TimeoutControl.SetTimeout(Constants.RequestBodyDrainTimeout.Ticks, TimeoutReason.RequestBodyDrain);

            try
            {
                ReadResult result;
                do
                {
                    result = await ReadAsync();
                    AdvanceTo(result.Buffer.End);
                } while (!result.IsCompleted);
            }
            catch (BadProtoRequestException ex)
            {
                _context.SetBadRequestState(ex);
            }
            catch (OperationCanceledException ex) when (ex is ConnectionAbortedException || ex is TaskCanceledException)
            {
                Log.RequestBodyDrainTimedOut(_context.ConnectionIdFeature, _context.TraceIdentifier);
            }
            catch (InvalidOperationException ex)
            {
                var connectionAbortedException = new ConnectionAbortedException(CoreStrings.ConnectionAbortedByApplication, ex);
                _context.ReportApplicationError(connectionAbortedException);

                // Have to abort the connection because we can't finish draining the request
                _context.StopProcessingNextRequest();
            }
            finally
            {
                _context.TimeoutControl.CancelTimeout();
            }
        }

        public static MessageBody For(
            ProtoVersion httpVersion,
            ProtoRequestHeaders headers,
            Proto1Connection context)
        {
            // see also http://tools.ietf.org/html/rfc2616#section-4.4
            var keepAlive = httpVersion != ProtoVersion.Proto10;

            var upgrade = false;
            if (headers.HasConnection)
            {
                var connectionOptions = ProtoHeaders.ParseConnection(headers.HeaderConnection);

                upgrade = (connectionOptions & ConnectionOptions.Upgrade) == ConnectionOptions.Upgrade;
                keepAlive = (connectionOptions & ConnectionOptions.KeepAlive) == ConnectionOptions.KeepAlive;
            }

            if (upgrade)
            {
                if (headers.HeaderTransferEncoding.Count > 0 || (headers.ContentLength.HasValue && headers.ContentLength.Value != 0))
                {
                    BadProtoRequestException.Throw(RequestRejectionReason.UpgradeRequestCannotHavePayload);
                }

                return new Proto1UpgradeMessageBody(context);
            }

            if (headers.HasTransferEncoding)
            {
                var transferEncoding = headers.HeaderTransferEncoding;
                var transferCoding = ProtoHeaders.GetFinalTransferCoding(transferEncoding);

                // https://tools.ietf.org/html/rfc7230#section-3.3.3
                // If a Transfer-Encoding header field
                // is present in a request and the chunked transfer coding is not
                // the final encoding, the message body length cannot be determined
                // reliably; the server MUST respond with the 400 (Bad Request)
                // status code and then close the connection.
                if (transferCoding != TransferCoding.Chunked)
                {
                    BadProtoRequestException.Throw(RequestRejectionReason.FinalTransferCodingNotChunked, transferEncoding);
                }

                // TODO may push more into the wrapper rather than just calling into the message body
                // NBD for now.
                return new Proto1ChunkedEncodingMessageBody(keepAlive, context);
            }

            if (headers.ContentLength.HasValue)
            {
                var contentLength = headers.ContentLength.Value;

                if (contentLength == 0)
                {
                    return keepAlive ? MessageBody.ZeroContentLengthKeepAlive : MessageBody.ZeroContentLengthClose;
                }

                return new Proto1ContentLengthMessageBody(keepAlive, contentLength, context);
            }

            // If we got here, request contains no Content-Length or Transfer-Encoding header.
            // Reject with 411 Length Required.
            if (context.Method == ProtoMethod.Post || context.Method == ProtoMethod.Put)
            {
                var requestRejectionReason = httpVersion == ProtoVersion.Proto11 ? RequestRejectionReason.LengthRequired : RequestRejectionReason.LengthRequiredProto10;
                BadProtoRequestException.Throw(requestRejectionReason, context.Method);
            }

            return keepAlive ? MessageBody.ZeroContentLengthKeepAlive : MessageBody.ZeroContentLengthClose;
        }
    }
}
