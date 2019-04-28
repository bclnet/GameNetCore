// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Proto.Features;
using Contoso.GameNetCore.Routing.Matching;

namespace Contoso.GameNetCore.Routing.TestObjects
{
    internal class TestMatcher : Matcher
    {
        private readonly bool _isHandled;

        public TestMatcher(bool isHandled)
        {
            _isHandled = isHandled;
        }

        public override Task MatchAsync(ProtoContext httpContext, EndpointSelectorContext context)
        {
            if (_isHandled)
            {
                context.RouteValues = new RouteValueDictionary(new { controller = "Home", action = "Index" });
                context.Endpoint = new Endpoint(TestConstants.EmptyRequestDelegate, EndpointMetadataCollection.Empty, "Test endpoint");
            }

            return Task.CompletedTask;
        }
    }
}
