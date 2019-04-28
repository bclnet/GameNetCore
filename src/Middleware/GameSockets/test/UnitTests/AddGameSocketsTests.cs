// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Contoso.GameNetCore.GameSockets.Test
{
    public class AddGameSocketsTests
    {
        [Fact]
        public void AddGameSocketsConfiguresOptions()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddGameSockets(o =>
            {
                o.KeepAliveInterval = TimeSpan.FromSeconds(1000);
                o.AllowedOrigins.Add("someString");
            });

            var services = serviceCollection.BuildServiceProvider();
            var socketOptions = services.GetRequiredService<IOptions<GameSocketOptions>>().Value;

            Assert.Equal(TimeSpan.FromSeconds(1000), socketOptions.KeepAliveInterval);
            Assert.Single(socketOptions.AllowedOrigins);
            Assert.Equal("someString", socketOptions.AllowedOrigins[0]);
        }
    }
}
