// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto
{
    public interface IProtoHeadersHandler
    {
        void OnHeader(Span<byte> name, Span<byte> value);
        void OnHeadersComplete();
    }
}