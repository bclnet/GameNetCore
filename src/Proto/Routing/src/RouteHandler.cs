// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Contoso.GameNetCore.Proto;

namespace Contoso.GameNetCore.Routing
{
    public class RouteHandler : IRouteHandler, IRouter
    {
        private readonly RequestDelegate _requestDelegate;

        public RouteHandler(RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public RequestDelegate GetRequestHandler(ProtoContext httpContext, RouteData routeData)
        {
            return _requestDelegate;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            // Nothing to do.
            return null;
        }

        public Task RouteAsync(RouteContext context)
        {
            context.Handler = _requestDelegate;
            return Task.CompletedTask;
        }
    }
}
