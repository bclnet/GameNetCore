// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Proto;

namespace Contoso.GameNetCore.Routing.Matching
{
    public abstract class PolicyJumpTable
    {
        public abstract int GetDestination(ProtoContext httpContext);

        internal virtual string DebuggerToString()
        {
            return GetType().Name;
        }
    }
}
