// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System.Net;
using Contoso.GameNetCore.Http;
using Contoso.GameNetCore.Rewrite.Internal.PatternSegments;
using Xunit;

namespace Contoso.GameNetCore.Rewrite.Tests.PatternSegments
{
    public class LocalAddressSegmentTests
    {
        [Fact]
        public void LocalAddress_AssertSegmentIsCorrect()
        {
            // Arrange
            var segement = new LocalAddressSegment();
            var context = new RewriteContext { HttpContext = new DefaultHttpContext() };
            context.HttpContext.Connection.LocalIpAddress = IPAddress.Parse("20.30.40.50");
            // Act
            var results = segement.Evaluate(context, null, null);

            // Assert
            Assert.Equal("20.30.40.50", results);
        }

        [Fact]
        public void LocalAddress_AssertNullLocalIpAddressReturnsNull()
        {
            var segement = new LocalAddressSegment();
            var context = new RewriteContext { HttpContext = new DefaultHttpContext() };
            context.HttpContext.Connection.LocalIpAddress = null;
            // Act
            var results = segement.Evaluate(context, null, null);

            // Assert
            Assert.Null( results);
        }
    }
}
