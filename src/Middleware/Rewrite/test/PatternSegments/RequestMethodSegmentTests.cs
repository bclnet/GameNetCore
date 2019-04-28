// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Http;
using Contoso.GameNetCore.Rewrite.Internal.PatternSegments;
using Xunit;

namespace Contoso.GameNetCore.Rewrite.Tests.PatternSegments
{
    public class RequestMethodSegmentTests
    {
        [Fact]
        public void RequestMethod_AssertSegmentIsCorrect()
        {
            // Arrange
            var segement = new RequestMethodSegment();
            var context = new RewriteContext { HttpContext = new DefaultHttpContext() };
            context.HttpContext.Request.Method = HttpMethods.Get;
            // Act
            var results = segement.Evaluate(context, null, null);

            // Assert
            Assert.Equal(HttpMethods.Get, results);
        }
    }
}
