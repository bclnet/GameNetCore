// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Proto;
using Moq;
using Xunit;

namespace Contoso.GameNetCore.Routing.Constraints
{
    public class ProtoMethodRouteConstraintTests
    {
        [Theory]
        [InlineData("GET")]
        [InlineData("PosT")]
        public void ProtoMethodRouteConstraint_IncomingRequest_AcceptsAllowedMethods(string httpMethod)
        {
            // Arrange
            var constraint = new ProtoMethodRouteConstraint("GET", "post");

            var httpContext = new DefaultProtoContext();
            httpContext.Request.Method = httpMethod;
            var route = Mock.Of<IRouter>();

            var values = new RouteValueDictionary(new { });

            // Act
            var result = constraint.Match(httpContext, route, "httpMethod", values, RouteDirection.IncomingRequest);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("OPTIONS")]
        [InlineData("SomeRandomThing")]
        public void ProtoMethodRouteConstraint_IncomingRequest_RejectsOtherMethods(string httpMethod)
        {
            // Arrange
            var constraint = new ProtoMethodRouteConstraint("GET", "post");

            var httpContext = new DefaultProtoContext();
            httpContext.Request.Method = httpMethod;
            var route = Mock.Of<IRouter>();

            var values = new RouteValueDictionary(new { });

            // Act
            var result = constraint.Match(httpContext, route, "httpMethod", values, RouteDirection.IncomingRequest);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("PosT")]
        public void ProtoMethodRouteConstraint_UrlGeneration_AcceptsAllowedMethods(string httpMethod)
        {
            // Arrange
            var constraint = new ProtoMethodRouteConstraint("GET", "post");

            var httpContext = new DefaultProtoContext();
            var route = Mock.Of<IRouter>();

            var values = new RouteValueDictionary(new { httpMethod = httpMethod });

            // Act
            var result = constraint.Match(httpContext, route, "httpMethod", values, RouteDirection.UrlGeneration);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("OPTIONS")]
        [InlineData("SomeRandomThing")]
        public void ProtoMethodRouteConstraint_UrlGeneration_RejectsOtherMethods(string httpMethod)
        {
            // Arrange
            var constraint = new ProtoMethodRouteConstraint("GET", "post");

            var httpContext = new DefaultProtoContext();
            var route = Mock.Of<IRouter>();

            var values = new RouteValueDictionary(new { httpMethod = httpMethod });

            // Act
            var result = constraint.Match(httpContext, route, "httpMethod", values, RouteDirection.UrlGeneration);

            // Assert
            Assert.False(result);
        }
    }
}
