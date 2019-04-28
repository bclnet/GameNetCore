// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BenchmarkDotNet.Attributes;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Routing.Tree;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.GameNetCore.Routing.LinkGeneration
{
    public class SingleRouteWithNoParametersBenchmark : EndpointRoutingBenchmarkBase
    {
        private TreeRouter _treeRouter;
        private LinkGenerator _linkGenerator;
        private (ProtoContext ProtoContext, RouteValueDictionary AmbientValues) _requestContext;

        [GlobalSetup]
        public void Setup()
        {
            var template = "Products/Details";
            var defaults = new { controller = "Products", action = "Details" };
            var requiredValues = new { controller = "Products", action = "Details" };

            // Endpoint routing related
            SetupEndpoints(CreateEndpoint(template, defaults, requiredValues: requiredValues));
            var services = CreateServices();
            _linkGenerator = services.GetRequiredService<LinkGenerator>();

            // Attribute routing related
            var treeRouteBuilder = services.GetRequiredService<TreeRouteBuilder>();
            CreateOutboundRouteEntry(treeRouteBuilder, Endpoints[0]);
            _treeRouter = treeRouteBuilder.Build();

            _requestContext = CreateCurrentRequestContext();
        }

        [Benchmark(Baseline = true)]
        public void TreeRouter()
        {
            var virtualPathData = _treeRouter.GetVirtualPath(new VirtualPathContext(
                _requestContext.ProtoContext,
                ambientValues: _requestContext.AmbientValues,
                values: new RouteValueDictionary(
                    new
                    {
                        controller = "Products",
                        action = "Details",
                    })));

            AssertUrl("/Products/Details", virtualPathData?.VirtualPath);
        }

        [Benchmark]
        public void EndpointRouting()
        {
            var actualUrl = _linkGenerator.GetPathByRouteValues(
                _requestContext.ProtoContext,
                routeName: null,
                values: new
                {
                    controller = "Products",
                    action = "Details",
                });

            AssertUrl("/Products/Details", actualUrl);
        }
    }
}
