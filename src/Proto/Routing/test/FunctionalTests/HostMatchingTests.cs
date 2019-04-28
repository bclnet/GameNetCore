// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Proto;
using System.Threading.Tasks;
using RoutingGameSite;
using Xunit;

namespace Contoso.GameNetCore.Routing.FunctionalTests
{
    public class HostMatchingTests : IClassFixture<RoutingTestFixture<UseEndpointRoutingStartup>>
    {
        private readonly RoutingTestFixture<UseEndpointRoutingStartup> _fixture;

        public HostMatchingTests(RoutingTestFixture<UseEndpointRoutingStartup> fixture)
        {
            _fixture = fixture;
        }

        private ProtoClient CreateClient(string baseAddress)
        {
            var client = _fixture.CreateClient(baseAddress);

            return client;
        }

        [Theory]
        [InlineData("http://localhost")]
        [InlineData("http://localhost:5001")]
        public async Task Get_CatchAll(string baseAddress)
        {
            // Arrange
            var request = new ProtoRequestMessage(ProtoMethod.Get, "api/DomainWildcard");

            // Act
            var client = CreateClient(baseAddress);
            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(ProtoStatusCode.OK, response.StatusCode);
            Assert.Equal("*:*", responseContent);
        }

        [Theory]
        [InlineData("http://9000.0.0.1")]
        [InlineData("http://9000.0.0.1:8888")]
        public async Task Get_MatchWildcardDomain(string baseAddress)
        {
            // Arrange
            var request = new ProtoRequestMessage(ProtoMethod.Get, "api/DomainWildcard");

            // Act
            var client = CreateClient(baseAddress);
            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(ProtoStatusCode.OK, response.StatusCode);
            Assert.Equal("*.0.0.1:*", responseContent);
        }

        [Theory]
        [InlineData("http://127.0.0.1")]
        [InlineData("http://127.0.0.1:8888")]
        public async Task Get_MatchDomain(string baseAddress)
        {
            // Arrange
            var request = new ProtoRequestMessage(ProtoMethod.Get, "api/DomainWildcard");

            // Act
            var client = CreateClient(baseAddress);
            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(ProtoStatusCode.OK, response.StatusCode);
            Assert.Equal("127.0.0.1:*", responseContent);
        }

        [Theory]
        [InlineData("http://9000.0.0.1:5000")]
        [InlineData("http://9000.0.0.1:5001")]
        public async Task Get_MatchWildcardDomainAndPort(string baseAddress)
        {
            // Arrange
            var request = new ProtoRequestMessage(ProtoMethod.Get, "api/DomainWildcard");

            // Act
            var client = CreateClient(baseAddress);
            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(ProtoStatusCode.OK, response.StatusCode);
            Assert.Equal("*.0.0.1:5000,*.0.0.1:5001", responseContent);
        }

        [Theory]
        [InlineData("http://www.contoso.com")]
        [InlineData("http://contoso.com")]
        public async Task Get_MatchWildcardDomainAndSubdomain(string baseAddress)
        {
            // Arrange
            var request = new ProtoRequestMessage(ProtoMethod.Get, "api/DomainWildcard");

            // Act
            var client = CreateClient(baseAddress);
            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(ProtoStatusCode.OK, response.StatusCode);
            Assert.Equal("contoso.com:*,*.contoso.com:*", responseContent);
        }
    }
}
