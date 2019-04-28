// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace Contoso.GameNetCore.Proto
{
    public class ProtoContextAccessor : IProtoContextAccessor
    {
        private static AsyncLocal<ProtoContextHolder> _httpContextCurrent = new AsyncLocal<ProtoContextHolder>();

        public ProtoContext ProtoContext
        {
            get
            {
                return  _httpContextCurrent.Value?.Context;
            }
            set
            {
                var holder = _httpContextCurrent.Value;
                if (holder != null)
                {
                    // Clear current ProtoContext trapped in the AsyncLocals, as its done.
                    holder.Context = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the ProtoContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    _httpContextCurrent.Value = new ProtoContextHolder { Context = value };
                }
            }
        }

        private class ProtoContextHolder
        {
            public ProtoContext Context;
        }
    }
}
