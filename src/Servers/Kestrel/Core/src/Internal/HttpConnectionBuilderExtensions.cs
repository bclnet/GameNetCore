// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Contoso.GameNetCore.Hosting.Server;
using Contoso.GameNetCore.Connections;
using Contoso.GameNetCore.Server.Kestrel.Core.Adapter.Internal;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal
{
    internal static class ProtoConnectionBuilderExtensions
    {
        public static IConnectionBuilder UseProtoServer<TContext>(this IConnectionBuilder builder, ServiceContext serviceContext, IProtoApplication<TContext> application, ProtoProtocols protocols)
        {
            return builder.UseProtoServer(Array.Empty<IConnectionAdapter>(), serviceContext, application, protocols);
        }

        public static IConnectionBuilder UseProtoServer<TContext>(this IConnectionBuilder builder, IList<IConnectionAdapter> adapters, ServiceContext serviceContext, IProtoApplication<TContext> application, ProtoProtocols protocols)
        {
            var middleware = new ProtoConnectionMiddleware<TContext>(adapters, serviceContext, application, protocols);
            return builder.Use(next =>
            {
                return middleware.OnConnectionAsync;
            });
        }
    }
}
