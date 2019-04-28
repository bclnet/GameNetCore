// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Routing.Patterns;
using Xunit;
using static Microsoft.GameNetCore.Routing.Matching.ProtoMethodMatcherPolicy;

namespace Contoso.GameNetCore.Routing.Matching
{
    public class ProtoMethodMatcherPolicyTest
    {
        [Fact]
        public void AppliesToNode_EndpointWithoutMetadata_ReturnsFalse()
        {
            // Arrange
            var endpoints = new[] { CreateEndpoint("/", null), };

            var policy = CreatePolicy();

            // Act
            var result = policy.AppliesToEndpoints(endpoints);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AppliesToNode_EndpointWithoutProtoMethods_ReturnsFalse()
        {
            // Arrange
            var endpoints = new[] 
            {
                CreateEndpoint("/", new ProtoMethodMetadata(Array.Empty<string>())),
            };

            var policy = CreatePolicy();

            // Act
            var result = policy.AppliesToEndpoints(endpoints);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AppliesToNode_EndpointHasProtoMethods_ReturnsTrue()
        {
            // Arrange
            var endpoints = new[]
            {
                CreateEndpoint("/", new ProtoMethodMetadata(Array.Empty<string>())),
                CreateEndpoint("/", new ProtoMethodMetadata(new[] { "GET", })),
            };

            var policy = CreatePolicy();

            // Act
            var result = policy.AppliesToEndpoints(endpoints);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetEdges_GroupsByProtoMethod()
        {
            // Arrange
            var endpoints = new[]
            {
                // These are arrange in an order that we won't actually see in a product scenario. It's done
                // this way so we can verify that ordering is preserved by GetEdges.
                CreateEndpoint("/", new ProtoMethodMetadata(new[] { "GET", })),
                CreateEndpoint("/", new ProtoMethodMetadata(Array.Empty<string>())),
                CreateEndpoint("/", new ProtoMethodMetadata(new[] { "GET", "PUT", "POST" })),
                CreateEndpoint("/", new ProtoMethodMetadata(new[] { "PUT", "POST" })),
                CreateEndpoint("/", new ProtoMethodMetadata(Array.Empty<string>())),
            };

            var policy = CreatePolicy();

            // Act
            var edges = policy.GetEdges(endpoints);

            // Assert
            Assert.Collection(
                edges.OrderBy(e => e.State),
                e =>
                {
                    Assert.Equal(new EdgeKey(AnyMethod, isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("GET", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[0], endpoints[1], endpoints[2], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("POST", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], endpoints[3], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("PUT", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], endpoints[3], endpoints[4], }, e.Endpoints.ToArray());
                });
        }

        [Fact]
        public void GetEdges_GroupsByProtoMethod_Cors()
        {
            // Arrange
            var endpoints = new[]
            {
                // These are arrange in an order that we won't actually see in a product scenario. It's done
                // this way so we can verify that ordering is preserved by GetEdges.
                CreateEndpoint("/", new ProtoMethodMetadata(new[] { "GET", })),
                CreateEndpoint("/", new ProtoMethodMetadata(Array.Empty<string>())),
                CreateEndpoint("/", new ProtoMethodMetadata(new[] { "GET", "PUT", "POST" }, acceptCorsPreflight: true)),
                CreateEndpoint("/", new ProtoMethodMetadata(new[] { "PUT", "POST" })),
                CreateEndpoint("/", new ProtoMethodMetadata(Array.Empty<string>(), acceptCorsPreflight: true)),
            };

            var policy = CreatePolicy();

            // Act
            var edges = policy.GetEdges(endpoints);

            // Assert
            Assert.Collection(
                edges.OrderBy(e => e.State),
                e =>
                {
                    Assert.Equal(new EdgeKey(AnyMethod, isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey(AnyMethod, isCorsPreflightRequest: true), e.State);
                    Assert.Equal(new[] { endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("GET", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[0], endpoints[1], endpoints[2], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("GET", isCorsPreflightRequest: true), e.State);
                    Assert.Equal(new[] { endpoints[2], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("POST", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], endpoints[3], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("POST", isCorsPreflightRequest: true), e.State);
                    Assert.Equal(new[] { endpoints[2], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("PUT", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], endpoints[3], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("PUT", isCorsPreflightRequest: true), e.State);
                    Assert.Equal(new[] { endpoints[2], endpoints[4], }, e.Endpoints.ToArray());
                });
        }

        [Fact] // See explanation in GetEdges for how this case is different
        public void GetEdges_GroupsByProtoMethod_CreatesProto405Endpoint()
        {
            // Arrange
            var endpoints = new[]
            {
                // These are arrange in an order that we won't actually see in a product scenario. It's done
                // this way so we can verify that ordering is preserved by GetEdges.
                CreateEndpoint("/", new ProtoMethodMetadata(new[] { "GET", })),
                CreateEndpoint("/", new ProtoMethodMetadata(new[] { "GET", "PUT", "POST" })),
                CreateEndpoint("/", new ProtoMethodMetadata(new[] { "PUT", "POST" })),
            };

            var policy = CreatePolicy();

            // Act
            var edges = policy.GetEdges(endpoints);

            // Assert
            Assert.Collection(
                edges.OrderBy(e => e.State),
                e =>
                {
                    Assert.Equal(new EdgeKey(AnyMethod, isCorsPreflightRequest: false), e.State);
                    Assert.Equal(Proto405EndpointDisplayName, e.Endpoints.Single().DisplayName);
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("GET", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[0], endpoints[1], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("POST", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("PUT", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], }, e.Endpoints.ToArray());
                });

        }

        [Fact] // See explanation in GetEdges for how this case is different
        public void GetEdges_GroupsByProtoMethod_CreatesProto405Endpoint_CORS()
        {
            // Arrange
            var endpoints = new[]
            {
                // These are arrange in an order that we won't actually see in a product scenario. It's done
                // this way so we can verify that ordering is preserved by GetEdges.
                CreateEndpoint("/", new ProtoMethodMetadata(new[] { "GET", })),
                CreateEndpoint("/", new ProtoMethodMetadata(new[] { "GET", "PUT", "POST" }, acceptCorsPreflight: true)),
                CreateEndpoint("/", new ProtoMethodMetadata(new[] { "PUT", "POST" })),
            };

            var policy = CreatePolicy();

            // Act
            var edges = policy.GetEdges(endpoints);

            // Assert
            Assert.Collection(
                edges.OrderBy(e => e.State),
                e =>
                {
                    Assert.Equal(new EdgeKey(AnyMethod, isCorsPreflightRequest: false), e.State);
                    Assert.Equal(Proto405EndpointDisplayName, e.Endpoints.Single().DisplayName);
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("GET", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[0], endpoints[1], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("GET", isCorsPreflightRequest: true), e.State);
                    Assert.Equal(new[] { endpoints[1], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("POST", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("POST", isCorsPreflightRequest: true), e.State);
                    Assert.Equal(new[] { endpoints[1], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("PUT", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("PUT", isCorsPreflightRequest: true), e.State);
                    Assert.Equal(new[] { endpoints[1], }, e.Endpoints.ToArray());
                });
        }

        private static RouteEndpoint CreateEndpoint(string template, ProtoMethodMetadata httpMethodMetadata)
        {
            var metadata = new List<object>();
            if (httpMethodMetadata != null)
            {
                metadata.Add(httpMethodMetadata);
            }

            return new RouteEndpoint(
                TestConstants.EmptyRequestDelegate,
                RoutePatternFactory.Parse(template),
                0,
                new EndpointMetadataCollection(metadata),
                $"test: {template}");
        }

        private static ProtoMethodMatcherPolicy CreatePolicy()
        {
            return new ProtoMethodMatcherPolicy();
        }
    }
}
