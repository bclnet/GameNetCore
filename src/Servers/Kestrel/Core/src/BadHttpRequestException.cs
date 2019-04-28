// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto;
using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace Contoso.GameNetCore.Server.Kestrel.Core
{
    public sealed class BadProtoRequestException : IOException
    {
        private BadProtoRequestException(string message, int statusCode, RequestRejectionReason reason)
            : this(message, statusCode, reason, null)
        { }

        private BadProtoRequestException(string message, int statusCode, RequestRejectionReason reason, ProtoMethod? requiredMethod)
            : base(message)
        {
            StatusCode = statusCode;
            Reason = reason;

            if (requiredMethod.HasValue)
            {
                AllowedHeader = ProtoUtilities.MethodToString(requiredMethod.Value);
            }
        }

        public int StatusCode { get; }

        internal StringValues AllowedHeader { get; }

        internal RequestRejectionReason Reason { get; }

        [StackTraceHidden]
        internal static void Throw(RequestRejectionReason reason)
        {
            throw GetException(reason);
        }

        [StackTraceHidden]
        internal static void Throw(RequestRejectionReason reason, ProtoMethod method)
            => throw GetException(reason, method.ToString().ToUpperInvariant());

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static BadProtoRequestException GetException(RequestRejectionReason reason)
        {
            BadProtoRequestException ex;
            switch (reason)
            {
                case RequestRejectionReason.InvalidRequestHeadersNoCRLF:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_InvalidRequestHeadersNoCRLF, StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.InvalidRequestLine:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_InvalidRequestLine, StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.MalformedRequestInvalidHeaders:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_MalformedRequestInvalidHeaders, StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.MultipleContentLengths:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_MultipleContentLengths, StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.UnexpectedEndOfRequestContent:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_UnexpectedEndOfRequestContent, StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.BadChunkSuffix:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_BadChunkSuffix, StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.BadChunkSizeData:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_BadChunkSizeData, StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.ChunkedRequestIncomplete:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_ChunkedRequestIncomplete, StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.InvalidCharactersInHeaderName:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_InvalidCharactersInHeaderName, StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.RequestLineTooLong:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_RequestLineTooLong, StatusCodes.Status414UriTooLong, reason);
                    break;
                case RequestRejectionReason.HeadersExceedMaxTotalSize:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_HeadersExceedMaxTotalSize, StatusCodes.Status431RequestHeaderFieldsTooLarge, reason);
                    break;
                case RequestRejectionReason.TooManyHeaders:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_TooManyHeaders, StatusCodes.Status431RequestHeaderFieldsTooLarge, reason);
                    break;
                case RequestRejectionReason.RequestBodyTooLarge:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_RequestBodyTooLarge, StatusCodes.Status413PayloadTooLarge, reason);
                    break;
                case RequestRejectionReason.RequestHeadersTimeout:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_RequestHeadersTimeout, StatusCodes.Status408RequestTimeout, reason);
                    break;
                case RequestRejectionReason.RequestBodyTimeout:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_RequestBodyTimeout, StatusCodes.Status408RequestTimeout, reason);
                    break;
                case RequestRejectionReason.OptionsMethodRequired:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_MethodNotAllowed, StatusCodes.Status405MethodNotAllowed, reason, ProtoMethod.Options);
                    break;
                case RequestRejectionReason.ConnectMethodRequired:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_MethodNotAllowed, StatusCodes.Status405MethodNotAllowed, reason, ProtoMethod.Connect);
                    break;
                case RequestRejectionReason.MissingHostHeader:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_MissingHostHeader, StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.MultipleHostHeaders:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_MultipleHostHeaders, StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.InvalidHostHeader:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_InvalidHostHeader, StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.UpgradeRequestCannotHavePayload:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest_UpgradeRequestCannotHavePayload, StatusCodes.Status400BadRequest, reason);
                    break;
                default:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest, StatusCodes.Status400BadRequest, reason);
                    break;
            }
            return ex;
        }

        [StackTraceHidden]
        internal static void Throw(RequestRejectionReason reason, string detail)
        {
            throw GetException(reason, detail);
        }

        [StackTraceHidden]
        internal static void Throw(RequestRejectionReason reason, StringValues detail)
        {
            throw GetException(reason, detail.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static BadProtoRequestException GetException(RequestRejectionReason reason, string detail)
        {
            BadProtoRequestException ex;
            switch (reason)
            {
                case RequestRejectionReason.InvalidRequestLine:
                    ex = new BadProtoRequestException(CoreStrings.FormatBadRequest_InvalidRequestLine_Detail(detail), StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.InvalidRequestTarget:
                    ex = new BadProtoRequestException(CoreStrings.FormatBadRequest_InvalidRequestTarget_Detail(detail), StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.InvalidRequestHeader:
                    ex = new BadProtoRequestException(CoreStrings.FormatBadRequest_InvalidRequestHeader_Detail(detail), StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.InvalidContentLength:
                    ex = new BadProtoRequestException(CoreStrings.FormatBadRequest_InvalidContentLength_Detail(detail), StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.UnrecognizedHTTPVersion:
                    ex = new BadProtoRequestException(CoreStrings.FormatBadRequest_UnrecognizedHTTPVersion(detail), StatusCodes.Status505ProtoVersionNotsupported, reason);
                    break;
                case RequestRejectionReason.FinalTransferCodingNotChunked:
                    ex = new BadProtoRequestException(CoreStrings.FormatBadRequest_FinalTransferCodingNotChunked(detail), StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.LengthRequired:
                    ex = new BadProtoRequestException(CoreStrings.FormatBadRequest_LengthRequired(detail), StatusCodes.Status411LengthRequired, reason);
                    break;
                case RequestRejectionReason.LengthRequiredProto10:
                    ex = new BadProtoRequestException(CoreStrings.FormatBadRequest_LengthRequiredProto10(detail), StatusCodes.Status400BadRequest, reason);
                    break;
                case RequestRejectionReason.InvalidHostHeader:
                    ex = new BadProtoRequestException(CoreStrings.FormatBadRequest_InvalidHostHeader_Detail(detail), StatusCodes.Status400BadRequest, reason);
                    break;
                default:
                    ex = new BadProtoRequestException(CoreStrings.BadRequest, StatusCodes.Status400BadRequest, reason);
                    break;
            }
            return ex;
        }
    }
}
