// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    /* https://tools.ietf.org/html/rfc7540#section-6.7
        +---------------------------------------------------------------+
        |                                                               |
        |                      Opaque Data (64)                         |
        |                                                               |
        +---------------------------------------------------------------+
    */
    internal partial class Proto2Frame
    {
        public Proto2PingFrameFlags PingFlags
        {
            get => (Proto2PingFrameFlags)Flags;
            set => Flags = (byte)value;
        }

        public bool PingAck => (PingFlags & Proto2PingFrameFlags.ACK) == Proto2PingFrameFlags.ACK;

        public void PreparePing(Proto2PingFrameFlags flags)
        {
            PayloadLength = 8;
            Type = Proto2FrameType.PING;
            PingFlags = flags;
            StreamId = 0;
        }
    }
}
