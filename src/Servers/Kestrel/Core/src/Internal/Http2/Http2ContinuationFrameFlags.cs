// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    [Flags]
    internal enum Proto2ContinuationFrameFlags : byte
    {
        NONE = 0x0,
        END_HEADERS = 0x4,
    }
}
