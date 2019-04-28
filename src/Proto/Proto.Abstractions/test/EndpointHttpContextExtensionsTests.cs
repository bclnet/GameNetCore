// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Contoso.GameNetCore.Proto.Endpoints;
using Contoso.GameNetCore.Proto.Features;
using Xunit;

namespace Contoso.GameNetCore.Proto.Abstractions.Tests
{
    public class EndpointProtoContextExtensionsTests
    {
        [Fact]
        public void GetEndpoint_ContextWithoutFeature_ReturnsNull()
        {
            // Arrange
            var context = new DefaultProtoContext();

            // Act
            var endpoint = context.GetEndpoint();

            // Assert
            Assert.Null(endpoint);
        }

        [Fact]
        public void GetEndpoint_ContextWithFeatureAndNullEndpoint_ReturnsNull()
        {
            // Arrange
            var context = new DefaultProtoContext();
            context.Features.Set<IEndpointFeature>(new EndpointFeature
            {
                Endpoint = null
            });

            // Act
            var endpoint = context.GetEndpoint();

            // Assert
            Assert.Null(endpoint);
        }

        [Fact]
        public void GetEndpoint_ContextWithFeatureAndEndpoint_ReturnsNull()
        {
            // Arrange
            var context = new DefaultProtoContext();
            var initial = new Endpoint(c => Task.CompletedTask, EndpointMetadataCollection.Empty, "Test endpoint");
            context.Features.Set<IEndpointFeature>(new EndpointFeature
            {
                Endpoint = initial
            });

            // Act
            var endpoint = context.GetEndpoint();

            // Assert
            Assert.Equal(initial, endpoint);
        }

        [Fact]
        public void SetEndpoint_NullOnContextWithoutFeature_NoFeatureSet()
        {
            // Arrange
            var context = new DefaultProtoContext();

            // Act
            context.SetEndpoint(null);

            // Assert
            Assert.Null(context.Features.Get<IEndpointFeature>());
        }

        [Fact]
        public void SetEndpoint_EndpointOnContextWithoutFeature_FeatureWithEndpointSet()
        {
            // Arrange
            var context = new DefaultProtoContext();

            // Act
            var endpoint = new Endpoint(c => Task.CompletedTask, EndpointMetadataCollection.Empty, "Test endpoint");
            context.SetEndpoint(endpoint);

            // Assert
            var feature = context.Features.Get<IEndpointFeature>();
            Assert.NotNull(feature);
            Assert.Equal(endpoint, feature.Endpoint);
        }

        [Fact]
        public void SetEndpoint_EndpointOnContextWithFeature_EndpointSetOnExistingFeature()
        {
            // Arrange
            var context = new DefaultProtoContext();
            var initialEndpoint = new Endpoint(c => Task.CompletedTask, EndpointMetadataCollection.Empty, "Test endpoint");
            var initialFeature = new EndpointFeature
            {
                Endpoint = initialEndpoint
            };
            context.Features.Set<IEndpointFeature>(initialFeature);

            // Act
            var endpoint = new Endpoint(c => Task.CompletedTask, EndpointMetadataCollection.Empty, "Test endpoint");
            context.SetEndpoint(endpoint);

            // Assert
            var feature = context.Features.Get<IEndpointFeature>();
            Assert.Equal(initialFeature, feature);
            Assert.Equal(endpoint, feature.Endpoint);
        }

        [Fact]
        public void SetEndpoint_NullOnContextWithFeature_NullSetOnExistingFeature()
        {
            // Arrange
            var context = new DefaultProtoContext();
            var initialEndpoint = new Endpoint(c => Task.CompletedTask, EndpointMetadataCollection.Empty, "Test endpoint");
            var initialFeature = new EndpointFeature
            {
                Endpoint = initialEndpoint
            };
            context.Features.Set<IEndpointFeature>(initialFeature);

            // Act
            context.SetEndpoint(null);

            // Assert
            var feature = context.Features.Get<IEndpointFeature>();
            Assert.Equal(initialFeature, feature);
            Assert.Null(feature.Endpoint);
        }

        [Fact]
        public void SetAndGetEndpoint_Roundtrip_EndpointIsRoundtrip()
        {
            // Arrange
            var context = new DefaultProtoContext();
            var initialEndpoint = new Endpoint(c => Task.CompletedTask, EndpointMetadataCollection.Empty, "Test endpoint");

            // Act
            context.SetEndpoint(initialEndpoint);
            var endpoint = context.GetEndpoint();

            // Assert
            Assert.Equal(initialEndpoint, endpoint);
        }

        private class EndpointFeature : IEndpointFeature
        {
            public Endpoint Endpoint { get; set; }
        }
    }
}
