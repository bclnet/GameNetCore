// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Routing.Patterns;

namespace Contoso.GameNetCore.Routing
{
    public sealed class RouteEndpointBuilder : EndpointBuilder
    {
        public RoutePattern RoutePattern { get; set; }

        public int Order { get; set; }

        public RouteEndpointBuilder(
           RequestDelegate requestDelegate,
           RoutePattern routePattern,
           int order)
        {
            RequestDelegate = requestDelegate;
            RoutePattern = routePattern;
            Order = order;
        }

        public override Endpoint Build()
        {
            var routeEndpoint = new RouteEndpoint(
                RequestDelegate,
                RoutePattern,
                Order,
                new EndpointMetadataCollection(Metadata),
                DisplayName);

            return routeEndpoint;
        }
    }
}
