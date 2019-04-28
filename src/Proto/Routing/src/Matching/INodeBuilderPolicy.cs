// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Contoso.GameNetCore.Proto;

namespace Contoso.GameNetCore.Routing.Matching
{
    public interface INodeBuilderPolicy
    {
        bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints);

        IReadOnlyList<PolicyNodeEdge> GetEdges(IReadOnlyList<Endpoint> endpoints);

        PolicyJumpTable BuildJumpTable(int exitDestination, IReadOnlyList<PolicyJumpTableEdge> edges);
    }
}
