// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Proto.Features;
using Contoso.GameNetCore.Routing.Tree;

namespace Contoso.GameNetCore.Routing.Matching
{
    // This is an adapter to use TreeRouter in the conformance tests
    internal class TreeRouterMatcher : Matcher
    {
        private readonly TreeRouter _inner;

        internal TreeRouterMatcher(TreeRouter inner)
        {
            _inner = inner;
        }

        public async override Task MatchAsync(ProtoContext httpContext, EndpointSelectorContext context)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var routeContext = new RouteContext(httpContext);
            await _inner.RouteAsync(routeContext);

            if (routeContext.Handler != null)
            {
                context.RouteValues = routeContext.RouteData.Values;
                await routeContext.Handler(httpContext);
            }
        }
    }
}

