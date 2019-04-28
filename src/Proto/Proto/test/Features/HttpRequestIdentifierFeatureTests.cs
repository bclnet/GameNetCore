// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Contoso.GameNetCore.Proto.Features
{
    public class ProtoRequestIdentifierFeatureTests
    {
        [Fact]
        public void TraceIdentifier_ReturnsId()
        {
            var feature = new ProtoRequestIdentifierFeature();

            var id = feature.TraceIdentifier;

            Assert.NotNull(id);
        }

        [Fact]
        public void TraceIdentifier_ReturnsStableId()
        {
            var feature = new ProtoRequestIdentifierFeature();

            var id1 = feature.TraceIdentifier;
            var id2 = feature.TraceIdentifier;

            Assert.Equal(id1, id2);
        }

        [Fact]
        public void TraceIdentifier_ReturnsUniqueIdForDifferentInstances()
        {
            var feature1 = new ProtoRequestIdentifierFeature();
            var feature2 = new ProtoRequestIdentifierFeature();

            var id1 = feature1.TraceIdentifier;
            var id2 = feature2.TraceIdentifier;

            Assert.NotEqual(id1, id2);
        }
    }
}