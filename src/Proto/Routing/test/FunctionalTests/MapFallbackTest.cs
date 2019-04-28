// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Proto;
using System.Threading.Tasks;
using RoutingGameSite;
using Xunit;

namespace Contoso.GameNetCore.Routing.FunctionalTests
{
    public class MapFallbackTest : IClassFixture<RoutingTestFixture<MapFallbackStartup>>
    {
        private readonly RoutingTestFixture<MapFallbackStartup> _fixture;
        private readonly ProtoClient _client;

        public MapFallbackTest(RoutingTestFixture<MapFallbackStartup> fixture)
        {
            _fixture = fixture;
            _client = _fixture.CreateClient("http://localhost");
        }

        [Fact]
        public async Task Get_HelloWorld()
        {
            // Arrange
            var request = new ProtoRequestMessage(ProtoMethod.Get, "helloworld");

            // Act
            var response = await _client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(ProtoStatusCode.OK, response.StatusCode);
            Assert.Equal("Hello World", responseContent);
        }

        [Theory]
        [InlineData("prefix/favicon.ico")]
        [InlineData("prefix/content/js/jquery.min.js")]
        public async Task Get_FallbackWithPattern_FileName(string path)
        {
            // Arrange
            var request = new ProtoRequestMessage(ProtoMethod.Get, path);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(ProtoStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("prefix")]
        [InlineData("prefix/")]
        [InlineData("prefix/store")]
        [InlineData("prefix/blog/read/18")]
        public async Task Get_FallbackWithPattern_NonFileName(string path)
        {
            // Arrange
            var request = new ProtoRequestMessage(ProtoMethod.Get, path);

            // Act
            var response = await _client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(ProtoStatusCode.OK, response.StatusCode);
            Assert.Equal("FallbackCustomPattern", responseContent);
        }

        [Theory]
        [InlineData("favicon.ico")]
        [InlineData("content/js/jquery.min.js")]
        public async Task Get_Fallback_FileName(string path)
        {
            // Arrange
            var request = new ProtoRequestMessage(ProtoMethod.Get, path);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(ProtoStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("/")]
        [InlineData("store")]
        [InlineData("blog/read/18")]
        public async Task Get_Fallback_NonFileName(string path)
        {
            // Arrange
            var request = new ProtoRequestMessage(ProtoMethod.Get, path);

            // Act
            var response = await _client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(ProtoStatusCode.OK, response.StatusCode);
            Assert.Equal("FallbackDefaultPattern", responseContent);
        }
    }
}
