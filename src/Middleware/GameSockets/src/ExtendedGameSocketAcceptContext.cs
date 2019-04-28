// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Http;
using System;

namespace Contoso.GameNetCore.GameSockets
{
    public class ExtendedGameSocketAcceptContext : GameSocketAcceptContext
    {
        public override string SubProtocol { get; set; }
        public int? ReceiveBufferSize { get; set; }
        public TimeSpan? KeepAliveInterval { get; set; }
    }
}