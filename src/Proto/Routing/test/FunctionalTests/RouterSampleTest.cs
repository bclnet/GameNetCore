// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Proto;
using System.Threading.Tasks;
using Contoso.GameNetCore.TestHost;
using RoutingGameSite;
using Xunit;

namespace Contoso.GameNetCore.Routing.FunctionalTests
{
    public class RouterSampleTest : IDisposable
    {
        private readonly ProtoClient _client;
        private readonly TestServer _testServer;

        public RouterSampleTest()
        {
            var gameHostBuilder = Program.GetGameHostBuilder(new[] { Program.RouterScenario, });
            _testServer = new TestServer(gameHostBuilder);
            _client = _testServer.CreateClient();
            _client.BaseAddress = new Uri("http://localhost");
        }

        [Theory]
        [InlineData("Branch1")]
        [InlineData("Branch2")]
        public async Task Routing_CanRouteRequest_ToBranchRouter(string branch)
        {
            // Arrange
            var message = new ProtoRequestMessage(ProtoMethod.Get, $"{branch}/api/get/5");

            // Act
            var response = await _client.SendAsync(message);

            // Assert
            Assert.Equal(ProtoStatusCode.OK, response.StatusCode);
            Assert.Equal($"{branch} - API Get 5", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Routing_CanRouteRequestDelegate_ToSpecificProtoVerb()
        {
            // Arrange
            var message = new ProtoRequestMessage(ProtoMethod.Get, "api/get/5");

            // Act
            var response = await _client.SendAsync(message);

            // Assert
            Assert.Equal(ProtoStatusCode.OK, response.StatusCode);
            Assert.Equal($"API Get 5", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Routing_CanRouteRequest_ToSpecificMiddleware()
        {
            // Arrange
            var message = new ProtoRequestMessage(ProtoMethod.Get, "api/middleware");

            // Act
            var response = await _client.SendAsync(message);

            // Assert
            Assert.Equal(ProtoStatusCode.OK, response.StatusCode);
            Assert.Equal($"Middleware!", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("PATCH")]
        [InlineData("DELETE")]
        [InlineData("HEAD")]
        [InlineData("OPTIONS")]
        public async Task Routing_CanRouteRequest_ToDefaultHandler(string httpVerb)
        {
            // Arrange
            var message = new ProtoRequestMessage(new ProtoMethod(httpVerb), "api/all/Joe/Duf");
            var expectedBody = $"Verb =  {httpVerb} - Path = /api/all/Joe/Duf - Route values - [name, Joe], [lastName, Duf]";

            // Act
            var response = await _client.SendAsync(message);

            // Assert
            Assert.Equal(ProtoStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedBody, body);
        }

        public void Dispose()
        {
            _testServer.Dispose();
            _client.Dispose();
        }
    }
}
