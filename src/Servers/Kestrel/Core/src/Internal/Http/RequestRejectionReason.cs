// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto
{
    internal enum RequestRejectionReason
    {
        UnrecognizedHTTPVersion,
        InvalidRequestLine,
        InvalidRequestHeader,
        InvalidRequestHeadersNoCRLF,
        MalformedRequestInvalidHeaders,
        InvalidContentLength,
        MultipleContentLengths,
        UnexpectedEndOfRequestContent,
        BadChunkSuffix,
        BadChunkSizeData,
        ChunkedRequestIncomplete,
        InvalidRequestTarget,
        InvalidCharactersInHeaderName,
        RequestLineTooLong,
        HeadersExceedMaxTotalSize,
        TooManyHeaders,
        RequestBodyTooLarge,
        RequestHeadersTimeout,
        RequestBodyTimeout,
        FinalTransferCodingNotChunked,
        LengthRequired,
        LengthRequiredProto10,
        OptionsMethodRequired,
        ConnectMethodRequired,
        MissingHostHeader,
        MultipleHostHeaders,
        InvalidHostHeader,
        UpgradeRequestCannotHavePayload,
        RequestBodyExceedsContentLength
    }
}
