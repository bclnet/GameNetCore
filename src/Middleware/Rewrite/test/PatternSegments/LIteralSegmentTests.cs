﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Rewrite.Internal.PatternSegments;
using Xunit;

namespace Contoso.GameNetCore.Rewrite.Tests.PatternSegments
{
    public class LiteralSegmentTests
    {
        [Fact]
        public void LiteralSegment_AssertSegmentIsCorrect()
        {
            // Arrange
            var segement = new LiteralSegment("foo");

            // Act
            var results = segement.Evaluate(null, null, null);

            // Assert
            Assert.Equal("foo", results);
        }
    }
}
