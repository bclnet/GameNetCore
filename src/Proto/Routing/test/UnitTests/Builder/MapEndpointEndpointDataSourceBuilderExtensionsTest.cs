// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Routing;
using Contoso.GameNetCore.Routing.Patterns;
using Moq;
using Xunit;

namespace Contoso.GameNetCore.Builder
{
    public class MapEndpointEndpointDataSourceBuilderExtensionsTest
    {
        private ModelEndpointDataSource GetBuilderEndpointDataSource(IEndpointRouteBuilder endpointRouteBuilder)
        {
            return Assert.IsType<ModelEndpointDataSource>(Assert.Single(endpointRouteBuilder.DataSources));
        }

        private RouteEndpointBuilder GetRouteEndpointBuilder(IEndpointRouteBuilder endpointRouteBuilder)
        {
            return Assert.IsType<RouteEndpointBuilder>(Assert.Single(GetBuilderEndpointDataSource(endpointRouteBuilder).EndpointBuilders));
        }

        [Fact]
        public void MapEndpoint_StringPattern_BuildsEndpoint()
        {
            // Arrange
            var builder = new DefaultEndpointRouteBuilder(Mock.Of<IApplicationBuilder>());
            RequestDelegate requestDelegate = (d) => null;

            // Act
            var endpointBuilder = builder.Map("/", requestDelegate);

            // Assert
            var endpointBuilder1 = GetRouteEndpointBuilder(builder);

            Assert.Equal(requestDelegate, endpointBuilder1.RequestDelegate);
            Assert.Equal("/", endpointBuilder1.DisplayName);
            Assert.Equal("/", endpointBuilder1.RoutePattern.RawText);
        }

        [Fact]
        public void MapEndpoint_TypedPattern_BuildsEndpoint()
        {
            // Arrange
            var builder = new DefaultEndpointRouteBuilder(Mock.Of<IApplicationBuilder>());
            RequestDelegate requestDelegate = (d) => null;

            // Act
            var endpointBuilder = builder.Map(RoutePatternFactory.Parse("/"), requestDelegate);

            // Assert
            var endpointBuilder1 = GetRouteEndpointBuilder(builder);

            Assert.Equal(requestDelegate, endpointBuilder1.RequestDelegate);
            Assert.Equal("/", endpointBuilder1.DisplayName);
            Assert.Equal("/", endpointBuilder1.RoutePattern.RawText);
        }

        [Fact]
        public void MapEndpoint_AttributesCollectedAsMetadata()
        {
            // Arrange
            var builder = new DefaultEndpointRouteBuilder(Mock.Of<IApplicationBuilder>());

            // Act
            var endpointBuilder = builder.Map(RoutePatternFactory.Parse("/"),  Handle);

            // Assert
            var endpointBuilder1 = GetRouteEndpointBuilder(builder);
            Assert.Equal("/", endpointBuilder1.RoutePattern.RawText);
            Assert.Equal(2, endpointBuilder1.Metadata.Count);
            Assert.IsType<Attribute1>(endpointBuilder1.Metadata[0]);
            Assert.IsType<Attribute2>(endpointBuilder1.Metadata[1]);
        }

        [Fact]
        public void MapEndpoint_GeneratedDelegateWorks()
        {
            // Arrange
            var builder = new DefaultEndpointRouteBuilder(Mock.Of<IApplicationBuilder>());

            Expression<RequestDelegate> handler = context => Task.CompletedTask;

            // Act
            var endpointBuilder = builder.Map(RoutePatternFactory.Parse("/"), handler.Compile());

            // Assert
            var endpointBuilder1 = GetRouteEndpointBuilder(builder);
            Assert.Equal("/", endpointBuilder1.RoutePattern.RawText);
        }

        [Fact]
        public void MapEndpoint_PrecedenceOfMetadata_BuilderMetadataReturned()
        {
            // Arrange
            var builder = new DefaultEndpointRouteBuilder(Mock.Of<IApplicationBuilder>());

            // Act
            var endpointBuilder = builder.MapMethods("/", new[] { "METHOD" }, HandleProtoMetdata);
            endpointBuilder.WithMetadata(new ProtoMethodMetadata(new[] { "BUILDER" }));

            // Assert
            var dataSource = Assert.Single(builder.DataSources);
            var endpoint = Assert.Single(dataSource.Endpoints);

            Assert.Equal(3, endpoint.Metadata.Count);
            Assert.Equal("ATTRIBUTE", GetMethod(endpoint.Metadata[0]));
            Assert.Equal("METHOD", GetMethod(endpoint.Metadata[1]));
            Assert.Equal("BUILDER", GetMethod(endpoint.Metadata[2]));

            Assert.Equal("BUILDER", endpoint.Metadata.GetMetadata<IProtoMethodMetadata>().ProtoMethods.Single());

            string GetMethod(object metadata)
            {
                var httpMethodMetadata = Assert.IsAssignableFrom<IProtoMethodMetadata>(metadata);
                return Assert.Single(httpMethodMetadata.ProtoMethods);
            }
        }

        [Attribute1]
        [Attribute2]
        private static Task Handle(ProtoContext context) => Task.CompletedTask;

        [ProtoMethod("ATTRIBUTE")]
        private static Task HandleProtoMetdata(ProtoContext context) => Task.CompletedTask;

        private class ProtoMethodAttribute : Attribute, IProtoMethodMetadata
        {
            public bool AcceptCorsPreflight => false;

            public IReadOnlyList<string> ProtoMethods { get; }

            public ProtoMethodAttribute(params string[] httpMethods)
            {
                ProtoMethods = httpMethods;
            }
        }

        private class Attribute1 : Attribute
        {
        }

        private class Attribute2 : Attribute
        {
        }

        private class Metadata
        {

        }
    }
}
