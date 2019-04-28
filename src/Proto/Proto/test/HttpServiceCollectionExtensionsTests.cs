// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Contoso.GameNetCore.Proto.Tests
{
    public class ProtoServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddProtoContextAccessor_AddsWithCorrectLifetime()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddProtoContextAccessor();

            // Assert
            var descriptor = services[0];
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
            Assert.Equal(typeof(ProtoContextAccessor), descriptor.ImplementationType);
        }

        [Fact]
        public void AddProtoContextAccessor_ThrowsWithoutServices()
        {
            Assert.Throws<ArgumentNullException>("services", () => ProtoServiceCollectionExtensions.AddProtoContextAccessor(null));
        }
    }
}
