// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Routing.Matching;
using Contoso.GameNetCore.Routing.Patterns;
using Contoso.GameNetCore.Routing.TestObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Contoso.GameNetCore.Routing
{
    internal static class EndpointFactory
    {
        public static RouteEndpoint CreateRouteEndpoint(
            string template,
            object defaults = null,
            object policies = null,
            object requiredValues = null,
            int order = 0,
            string displayName = null,
            params object[] metadata)
        {
            var routePattern = RoutePatternFactory.Parse(template, defaults, policies, requiredValues);

            return CreateRouteEndpoint(routePattern, order, displayName, metadata);
        }

        public static RouteEndpoint CreateRouteEndpoint(
            RoutePattern routePattern = null,
            int order = 0,
            string displayName = null,
            IList<object> metadata = null)
        {
            return new RouteEndpoint(
                TestConstants.EmptyRequestDelegate,
                routePattern,
                order,
                new EndpointMetadataCollection(metadata ?? Array.Empty<object>()),
                displayName);
        }
    }
}
