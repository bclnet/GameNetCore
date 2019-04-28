// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto
{
    internal readonly struct Proto1ParsingHandler : IProtoRequestLineHandler, IProtoHeadersHandler
    {
        public readonly Proto1Connection Connection;

        public Proto1ParsingHandler(Proto1Connection connection)
        {
            Connection = connection;
        }

        public void OnHeader(Span<byte> name, Span<byte> value)
            => Connection.OnHeader(name, value);

        public void OnHeadersComplete()
            => Connection.OnHeadersComplete();

        public void OnStartLine(ProtoMethod method, ProtoVersion version, Span<byte> target, Span<byte> path, Span<byte> query, Span<byte> customMethod, bool pathEncoded)
            => Connection.OnStartLine(method, version, target, path, query, customMethod, pathEncoded);
    }
}
