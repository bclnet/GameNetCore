// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    /* https://tools.ietf.org/html/rfc7540#section-6.10
        +---------------------------------------------------------------+
        |                   Header Block Fragment (*)                 ...
        +---------------------------------------------------------------+
    */
    internal partial class Proto2Frame
    {
        public Proto2ContinuationFrameFlags ContinuationFlags
        {
            get => (Proto2ContinuationFrameFlags)Flags;
            set => Flags = (byte)value;
        }

        public bool ContinuationEndHeaders => (ContinuationFlags & Proto2ContinuationFrameFlags.END_HEADERS) == Proto2ContinuationFrameFlags.END_HEADERS;

        public void PrepareContinuation(Proto2ContinuationFrameFlags flags, int streamId)
        {
            PayloadLength = 0;
            Type = Proto2FrameType.CONTINUATION;
            ContinuationFlags = flags;
            StreamId = streamId;
        }
    }
}
