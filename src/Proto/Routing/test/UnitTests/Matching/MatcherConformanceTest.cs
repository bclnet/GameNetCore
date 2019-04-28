// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Proto.Features;
using Contoso.GameNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.GameNetCore.Routing.Matching
{
    public abstract partial class MatcherConformanceTest
    {
        internal abstract Matcher CreateMatcher(params RouteEndpoint[] endpoints);

        internal static (ProtoContext httpContext, EndpointSelectorContext context) CreateContext(string path)
        {
            var httpContext = new DefaultProtoContext();
            httpContext.Request.Method = "TEST";
            httpContext.Request.Path = path;
            httpContext.RequestServices = CreateServices();

            var context = new EndpointSelectorContext()
            {
                RouteValues = new RouteValueDictionary()
            };
            httpContext.Features.Set<IEndpointFeature>(context);
            httpContext.Features.Set<IRouteValuesFeature>(context);

            return (httpContext, context);
        }

        // The older routing implementations retrieve services when they first execute.
        internal static IServiceProvider CreateServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            return services.BuildServiceProvider();
        }

        internal static RouteEndpoint CreateEndpoint(
            string template, 
            object defaults = null,
            object constraints = null,
            int? order = null)
        {
            return new RouteEndpoint(
                TestConstants.EmptyRequestDelegate,
                RoutePatternFactory.Parse(template, defaults, constraints),
                order ?? 0,
                EndpointMetadataCollection.Empty,
                "endpoint: " + template);
        }

        internal (Matcher matcher, RouteEndpoint endpoint) CreateMatcher(string template)
        {
            var endpoint = CreateEndpoint(template);
            return (CreateMatcher(endpoint), endpoint);
        }
    }
}
