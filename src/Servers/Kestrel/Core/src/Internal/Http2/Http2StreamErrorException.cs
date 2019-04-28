// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    internal class Proto2StreamErrorException : Exception
    {
        public Proto2StreamErrorException(int streamId, string message, Proto2ErrorCode errorCode)
            : base($"HTTP/2 stream ID {streamId} error ({errorCode}): {message}")
        {
            StreamId = streamId;
            ErrorCode = errorCode;
        }

        public int StreamId { get; }

        public Proto2ErrorCode ErrorCode { get; }
    }
}
