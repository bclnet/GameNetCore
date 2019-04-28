// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    /* https://tools.ietf.org/html/rfc7540#section-4.1
        +-----------------------------------------------+
        |                 Length (24)                   |
        +---------------+---------------+---------------+
        |   Type (8)    |   Flags (8)   |
        +-+-------------+---------------+-------------------------------+
        |R|                 Stream Identifier (31)                      |
        +=+=============================================================+
        |                   Frame Payload (0...)                      ...
        +---------------------------------------------------------------+
    */
    internal partial class Proto2Frame
    {
        public int PayloadLength { get; set; }

        public Proto2FrameType Type { get; set; }

        public byte Flags { get; set; }

        public int StreamId { get; set; }

        internal object ShowFlags()
        {
            switch (Type)
            {
                case Proto2FrameType.CONTINUATION:
                    return ContinuationFlags;
                case Proto2FrameType.DATA:
                    return DataFlags;
                case Proto2FrameType.HEADERS:
                    return HeadersFlags;
                case Proto2FrameType.SETTINGS:
                    return SettingsFlags;
                case Proto2FrameType.PING:
                    return PingFlags;

                // Not Implemented
                case Proto2FrameType.PUSH_PROMISE:

                // No flags defined
                case Proto2FrameType.PRIORITY:
                case Proto2FrameType.RST_STREAM:
                case Proto2FrameType.GOAWAY:
                case Proto2FrameType.WINDOW_UPDATE:
                default:
                    return $"0x{Flags:x}";
            }
        }

        public override string ToString()
        {
            return $"{Type} Stream: {StreamId} Length: {PayloadLength} Flags: {ShowFlags()}";
        }
    }
}
