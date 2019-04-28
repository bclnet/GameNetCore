// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Proto.Features;

namespace Contoso.GameNetCore.Proto
{
    public static class ResponseExtensions
    {
        public static void Clear(this ProtoResponse response)
        {
            if (response.HasStarted)
            {
                throw new InvalidOperationException("The response cannot be cleared, it has already started sending.");
            }
            response.StatusCode = 200;
            response.ProtoContext.Features.Get<IProtoResponseFeature>().ReasonPhrase = null;
            response.Headers.Clear();
            if (response.Body.CanSeek)
            {
                response.Body.SetLength(0);
            }
        }
    }
}
