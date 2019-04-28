// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    /* https://tools.ietf.org/html/rfc7540#section-6.8
        +-+-------------------------------------------------------------+
        |R|                  Last-Stream-ID (31)                        |
        +-+-------------------------------------------------------------+
        |                      Error Code (32)                          |
        +---------------------------------------------------------------+
        |                  Additional Debug Data (*)                    |
        +---------------------------------------------------------------+
    */
    internal partial class Proto2Frame
    {
        public int GoAwayLastStreamId { get; set; }

        public Proto2ErrorCode GoAwayErrorCode { get; set; }

        public void PrepareGoAway(int lastStreamId, Proto2ErrorCode errorCode)
        {
            PayloadLength = 8;
            Type = Proto2FrameType.GOAWAY;
            Flags = 0;
            StreamId = 0;
            GoAwayLastStreamId = lastStreamId;
            GoAwayErrorCode = errorCode;
        }
    }
}
