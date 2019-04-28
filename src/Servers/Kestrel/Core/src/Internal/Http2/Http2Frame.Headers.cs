// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    /* https://tools.ietf.org/html/rfc7540#section-6.2
        +---------------+
        |Pad Length? (8)|
        +-+-------------+-----------------------------------------------+
        |E|                 Stream Dependency? (31)                     |
        +-+-------------+-----------------------------------------------+
        |  Weight? (8)  |
        +-+-------------+-----------------------------------------------+
        |                   Header Block Fragment (*)                 ...
        +---------------------------------------------------------------+
        |                           Padding (*)                       ...
        +---------------------------------------------------------------+
    */
    internal partial class Proto2Frame
    {
        public Proto2HeadersFrameFlags HeadersFlags
        {
            get => (Proto2HeadersFrameFlags)Flags;
            set => Flags = (byte)value;
        }

        public bool HeadersEndHeaders => (HeadersFlags & Proto2HeadersFrameFlags.END_HEADERS) == Proto2HeadersFrameFlags.END_HEADERS;

        public bool HeadersEndStream => (HeadersFlags & Proto2HeadersFrameFlags.END_STREAM) == Proto2HeadersFrameFlags.END_STREAM;

        public bool HeadersHasPadding => (HeadersFlags & Proto2HeadersFrameFlags.PADDED) == Proto2HeadersFrameFlags.PADDED;

        public bool HeadersHasPriority => (HeadersFlags & Proto2HeadersFrameFlags.PRIORITY) == Proto2HeadersFrameFlags.PRIORITY;

        public byte HeadersPadLength { get; set; }

        public int HeadersStreamDependency { get; set; }

        public byte HeadersPriorityWeight { get; set; }

        private int HeadersPayloadOffset => (HeadersHasPadding ? 1 : 0) + (HeadersHasPriority ? 5 : 0);

        public int HeadersPayloadLength => PayloadLength - HeadersPayloadOffset - HeadersPadLength;

        public void PrepareHeaders(Proto2HeadersFrameFlags flags, int streamId)
        {
            PayloadLength = 0;
            Type = Proto2FrameType.HEADERS;
            HeadersFlags = flags;
            StreamId = streamId;
        }
    }
}
