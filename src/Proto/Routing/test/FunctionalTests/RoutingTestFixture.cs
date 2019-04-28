// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Proto;
using Contoso.GameNetCore.Hosting;
using Contoso.GameNetCore.TestHost;

namespace Contoso.GameNetCore.Routing.FunctionalTests
{
    public class RoutingTestFixture<TStartup> : IDisposable
    {
        private readonly TestServer _server;

        public RoutingTestFixture()
        {
            var builder = new GameHostBuilder()
                .UseStartup(typeof(TStartup));

            _server = new TestServer(builder);

            Client = _server.CreateClient();
            Client.BaseAddress = new Uri("http://localhost");
        }

        public ProtoClient Client { get; }

        public ProtoClient CreateClient(string baseAddress)
        {
            var client = _server.CreateClient();
            client.BaseAddress = new Uri(baseAddress);

            return client;
        }

        public void Dispose()
        {
            Client.Dispose();
            _server.Dispose();
        }
    }
}
