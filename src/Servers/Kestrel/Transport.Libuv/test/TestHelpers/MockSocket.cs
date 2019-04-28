// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Server.Kestrel.Transport.Libuv.Internal;
using Contoso.GameNetCore.Server.Kestrel.Transport.Libuv.Internal.Networking;

namespace Contoso.GameNetCore.Server.Kestrel.Transport.Libuv.Tests.TestHelpers
{
    class MockSocket : UvStreamHandle
    {
        public MockSocket(LibuvFunctions uv, int threadId, ILibuvTrace logger) : base(logger)
        {
            CreateMemory(uv, threadId, IntPtr.Size);
        }

        protected override bool ReleaseHandle()
        {
            DestroyMemory(handle);
            handle = IntPtr.Zero;
            return true;
        }
    }
}
