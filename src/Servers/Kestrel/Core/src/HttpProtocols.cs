// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Contoso.GameNetCore.Server.Kestrel.Core
{
    [Flags]
    public enum ProtoProtocols
    {
        None = 0x0,
        Proto1 = 0x1,
        Proto2 = 0x2,
        Proto1AndProto2 = Proto1 | Proto2,
    }
}
