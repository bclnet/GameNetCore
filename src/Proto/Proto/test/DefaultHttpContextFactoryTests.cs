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
    public class DefaultProtoContextFactoryTests
    {
        [Fact]
        public void CreateProtoContextSetsProtoContextAccessor()
        {
            // Arrange
            var services = new ServiceCollection()
                .AddOptions()
                .AddProtoContextAccessor()
                .BuildServiceProvider();
            var accessor = services.GetRequiredService<IProtoContextAccessor>();
            var contextFactory = new DefaultProtoContextFactory(services);

            // Act
            var context = contextFactory.Create(new FeatureCollection());

            // Assert
            Assert.Same(context, accessor.ProtoContext);
        }

        [Fact]
        public void DisposeProtoContextSetsProtoContextAccessorToNull()
        {
            // Arrange
            var services = new ServiceCollection()
                .AddOptions()
                .AddProtoContextAccessor()
                .BuildServiceProvider();
            var accessor = services.GetRequiredService<IProtoContextAccessor>();
            var contextFactory = new DefaultProtoContextFactory(services);

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
            var services = new ServiceCollection()
                .AddOptions()
                .BuildServiceProvider();
            var contextFactory = new DefaultProtoContextFactory(services);

            // Act & Assert
            var context = contextFactory.Create(new FeatureCollection());
            contextFactory.Dispose(context);
        }

        [Fact]
        public void SetsDefaultPropertiesOnProtoContext()
        {
            // Arrange
            var services = new ServiceCollection()
                .AddOptions()
                .BuildServiceProvider();
            var contextFactory = new DefaultProtoContextFactory(services);

            // Act & Assert
            var context = contextFactory.Create(new FeatureCollection()) as DefaultProtoContext;
            Assert.NotNull(context);
            Assert.NotNull(context.FormOptions);
            Assert.NotNull(context.ServiceScopeFactory);

            Assert.Same(services.GetRequiredService<IServiceScopeFactory>(), context.ServiceScopeFactory);
        }
    }
}
