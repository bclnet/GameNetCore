﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Routing.Matching;
using Contoso.GameNetCore.Routing.Patterns;
using Xunit;

namespace Contoso.GameNetCore.Routing
{
    public class EndpointSelectorContextTest
    {
        [Fact]
        public void RouteData_CanIntializeDataTokens_WithMetadata()
        {
            // Arrange
            var expected = new RouteValueDictionary(new { foo = 17, bar = "hello", });

            var context = new EndpointSelectorContext()
            {
                Endpoint = new RouteEndpoint(
                    TestConstants.EmptyRequestDelegate,
                    RoutePatternFactory.Parse("/"),
                    0,
                    new EndpointMetadataCollection(new DataTokensMetadata(expected)),
                    "test"),
            };

            // Act
            var routeData = ((IRoutingFeature)context).RouteData;

            // Assert
            Assert.NotSame(expected, routeData.DataTokens);
            Assert.Equal(expected.OrderBy(kvp => kvp.Key), routeData.DataTokens.OrderBy(kvp => kvp.Key));
        }

        [Fact]
        public void RouteData_DataTokensIsEmpty_WithoutMetadata()
        {
            // Arrange
            var context = new EndpointSelectorContext()
            {
                Endpoint = new RouteEndpoint(
                    TestConstants.EmptyRequestDelegate,
                    RoutePatternFactory.Parse("/"),
                    0,
                    new EndpointMetadataCollection(),
                    "test"),
            };

            // Act
            var routeData = ((IRoutingFeature)context).RouteData;

            // Assert
            Assert.Empty(routeData.DataTokens);
        }
    }
}
