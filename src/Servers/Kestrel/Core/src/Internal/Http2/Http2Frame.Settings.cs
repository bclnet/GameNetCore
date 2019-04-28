// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    /* https://tools.ietf.org/html/rfc7540#section-6.5.1
        List of:
        +-------------------------------+
        |       Identifier (16)         |
        +-------------------------------+-------------------------------+
        |                        Value (32)                             |
        +---------------------------------------------------------------+
    */
    internal partial class Proto2Frame
    {
        public Proto2SettingsFrameFlags SettingsFlags
        {
            get => (Proto2SettingsFrameFlags)Flags;
            set => Flags = (byte)value;
        }

        public bool SettingsAck => (SettingsFlags & Proto2SettingsFrameFlags.ACK) == Proto2SettingsFrameFlags.ACK;

        public void PrepareSettings(Proto2SettingsFrameFlags flags)
        {
            PayloadLength = 0;
            Type = Proto2FrameType.SETTINGS;
            SettingsFlags = flags;
            StreamId = 0;
        }
    }
}
