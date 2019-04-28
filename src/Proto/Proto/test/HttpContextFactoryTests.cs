#pragma warning disable CS0618 // Type or member is obsolete
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Contoso.GameNetCore.Proto.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Contoso.GameNetCore.Proto
{
    public class ProtoContextFactoryTests
    {
        [Fact]
        public void ConstructorWithoutServiceScopeFactoryThrows()
        {
            // Arrange
            var accessor = new ProtoContextAccessor();
            var exception1 = Assert.Throws<ArgumentNullException>(() => new ProtoContextFactory(Options.Create(new FormOptions()), accessor));
            var exception2 = Assert.Throws<ArgumentNullException>(() => new ProtoContextFactory(Options.Create(new FormOptions())));

            Assert.Equal("serviceScopeFactory", exception1.ParamName);
            Assert.Equal("serviceScopeFactory", exception2.ParamName);
        }

        [Fact]
        public void CreateProtoContextSetsProtoContextAccessor()
        {
            // Arrange
            var accessor = new ProtoContextAccessor();
            var contextFactory = new ProtoContextFactory(Options.Create(new FormOptions()), new MyServiceScopeFactory(), accessor);

            // Act
            var context = contextFactory.Create(new FeatureCollection());

            // Assert
            Assert.Same(context, accessor.ProtoContext);
        }

        [Fact]
        public void DisposeProtoContextSetsProtoContextAccessorToNull()
        {
            // Arrange
            var accessor = new ProtoContextAccessor();
            var contextFactory = new ProtoContextFactory(Options.Create(new FormOptions()), new MyServiceScopeFactory(), accessor);

            // Act
            var context = contextFactory.Create(new FeatureCollection());

            // Assert
            Assert.Same(context, accessor.ProtoContext);

            contextFactory.Dispose(context);

            Assert.Null(accessor.ProtoContext);
        }

        [Fact]
        public void AllowsCreatingContextWithoutSettingAccessor()
        {
            // Arrange
            var contextFactory = new ProtoContextFactory(Options.Create(new FormOptions()), new MyServiceScopeFactory());

            // Act & Assert
            var context = contextFactory.Create(new FeatureCollection());
            contextFactory.Dispose(context);
        }

        private class MyServiceScopeFactory : IServiceScopeFactory
        {
            public IServiceScope CreateScope() => null;
        }
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
