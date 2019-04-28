// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Proto;
using System.Threading.Tasks;
using Contoso.GameNetCore.TestHost;
using Xunit;

namespace Contoso.GameNetCore.Routing.FunctionalTests
{
    public class EndpointRoutingBenchmarkTest : IDisposable
    {
        private readonly ProtoClient _client;
        private readonly TestServer _testServer;

        public EndpointRoutingBenchmarkTest()
        {
            // This switch and value are set by benchmark server when running the app for profiling.
            var args = new[] { "--scenarios", "PlaintextEndpointRouting" };
            var gameHostBuilder = Benchmarks.Program.GetGameHostBuilder(args);

            // Make sure we are using the right startup
            var startupName = gameHostBuilder.GetSetting("Startup");
            Assert.Equal(nameof(Benchmarks.StartupUsingEndpointRouting), startupName);

            _testServer = new TestServer(gameHostBuilder);
            _client = _testServer.CreateClient();
            _client.BaseAddress = new Uri("http://localhost");
        }

        [Fact]
        public async Task RouteEndpoint_ReturnsPlaintextResponse()
        {
            // Arrange
            var expectedContentType = "text/plain";
            var expectedContent = "Hello, World!";

            // Act
            var response = await _client.GetAsync("/plaintext");

            // Assert
            Assert.Equal(ProtoStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal(expectedContentType, response.Content.Headers.ContentType.MediaType);
            var actualContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedContent, actualContent);
        }

        public void Dispose()
        {
            _testServer.Dispose();
            _client.Dispose();
        }
    }
}
