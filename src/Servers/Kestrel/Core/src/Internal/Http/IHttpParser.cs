// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto
{
    public interface IProtoParser<TRequestHandler> where TRequestHandler : IProtoHeadersHandler, IProtoRequestLineHandler
    {
        bool ParseRequestLine(TRequestHandler handler, in ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined);

        bool ParseHeaders(TRequestHandler handler, in ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined, out int consumedBytes);
    }
}
