// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Proto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Contoso.GameNetCore.Routing.Tests
{
    public class RouteOptionsTests
    {
        [Fact]
        public void ConfigureRouting_ConfiguresOptionsProperly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();

            // Act
            services.AddRouting(options => options.ConstraintMap.Add("foo", typeof(TestRouteConstraint)));
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var accessor = serviceProvider.GetRequiredService<IOptions<RouteOptions>>();
            Assert.Equal("TestRouteConstraint", accessor.Value.ConstraintMap["foo"].Name);
        }

        private class TestRouteConstraint : IRouteConstraint
        {
            public TestRouteConstraint(string pattern)
            {
                Pattern = pattern;
            }

            public string Pattern { get; private set; }
            public bool Match(
                ProtoContext httpContext,
                IRouter route,
                string routeKey,
                RouteValueDictionary values,
                RouteDirection routeDirection)
            {
                throw new NotImplementedException();
            }
        }
    }
}
