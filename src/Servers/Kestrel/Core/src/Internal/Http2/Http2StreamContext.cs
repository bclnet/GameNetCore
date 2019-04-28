// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2.FlowControl;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    internal class Proto2StreamContext : ProtoConnectionContext
    {
        public int StreamId { get; set; }
        public IProto2StreamLifetimeHandler StreamLifetimeHandler { get; set; }
        public Proto2PeerSettings ClientPeerSettings { get; set; }
        public Proto2PeerSettings ServerPeerSettings { get; set; }
        public Proto2FrameWriter FrameWriter { get; set; }
        public InputFlowControl ConnectionInputFlowControl { get; set; }
        public OutputFlowControl ConnectionOutputFlowControl { get; set; }
    }
}
