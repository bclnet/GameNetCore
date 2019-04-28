// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Routing;
using Contoso.GameNetCore.Routing.Logging;
using Microsoft.Extensions.Logging;

namespace Contoso.GameNetCore.Builder
{
    public class RouterMiddleware
    {
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;
        private readonly IRouter _router;

        public RouterMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory,
            IRouter router)
        {
            _next = next;
            _router = router;

            _logger = loggerFactory.CreateLogger<RouterMiddleware>();
        }

        public async Task Invoke(ProtoContext httpContext)
        {
            var context = new RouteContext(httpContext);
            context.RouteData.Routers.Add(_router);

            await _router.RouteAsync(context);

            if (context.Handler == null)
            {
                _logger.RequestNotMatched();
                await _next.Invoke(httpContext);
            }
            else
            {
                httpContext.Features[typeof(IRoutingFeature)] = new RoutingFeature()
                {
                    RouteData = context.RouteData,
                };

                await context.Handler(context.ProtoContext);
            }
        }
    }
}
