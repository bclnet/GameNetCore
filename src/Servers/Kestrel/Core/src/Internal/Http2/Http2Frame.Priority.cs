// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    /* https://tools.ietf.org/html/rfc7540#section-6.3
        +-+-------------------------------------------------------------+
        |E|                  Stream Dependency (31)                     |
        +-+-------------+-----------------------------------------------+
        |   Weight (8)  |
        +-+-------------+
    */
    internal partial class Proto2Frame
    {
        public int PriorityStreamDependency { get; set; }

        public bool PriorityIsExclusive { get; set; }

        public byte PriorityWeight { get; set; }

        public void PreparePriority(int streamId, int streamDependency, bool exclusive, byte weight)
        {
            PayloadLength = 5;
            Type = Proto2FrameType.PRIORITY;
            StreamId = streamId;
            PriorityStreamDependency = streamDependency;
            PriorityIsExclusive = exclusive;
            PriorityWeight = weight;
        }
    }
}
