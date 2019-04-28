// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Proto;
using System.Threading.Tasks;
using Contoso.GameNetCore.Hosting;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Contoso.GameNetCore.Routing.FunctionalTests
{
    public class GameHostBuilderExtensionsTest
    {
        public static TheoryData<Action<IRouteBuilder>, ProtoRequestMessage, string> MatchesRequest
        {
            get
            {
                return new TheoryData<Action<IRouteBuilder>, ProtoRequestMessage, string>()
                {
                    {
                        (rb) => rb.MapGet("greeting/{name}", (req, resp, routeData) => resp.WriteAsync($"Hello! {routeData.Values["name"]}")),
                        new ProtoRequestMessage(ProtoMethod.Get, "greeting/James"),
                        "Hello! James"
                    },
                    {
                        (rb) => rb.MapPost(
                            "greeting/{name}",
                            async (req, resp, routeData) =>
                            {
                                var streamReader = new StreamReader(req.Body);
                                var data = await streamReader.ReadToEndAsync();
                                await resp.WriteAsync($"{routeData.Values["name"]} {data}");
                            }),
                        new ProtoRequestMessage(ProtoMethod.Post, "greeting/James") { Content = new StringContent("Biography") },
                        "James Biography"
                    },
                    {
                        (rb) => rb.MapPut(
                            "greeting/{name}",
                            async (req, resp, routeData) =>
                            {
                                var streamReader = new StreamReader(req.Body);
                                var data = await streamReader.ReadToEndAsync();
                                await resp.WriteAsync($"{routeData.Values["name"]} {data}");
                            }),
                        new ProtoRequestMessage(ProtoMethod.Put, "greeting/James") { Content = new StringContent("Biography") },
                        "James Biography"
                    },
                    {
                        (rb) => rb.MapDelete("greeting/{name}", (req, resp, routeData) => resp.WriteAsync($"Hello! {routeData.Values["name"]}")),
                        new ProtoRequestMessage(ProtoMethod.Delete, "greeting/James"),
                        "Hello! James"
                    },
                    {
                        (rb) => rb.MapVerb(
                            "POST",
                            "greeting/{name}",
                            async (req, resp, routeData) =>
                            {
                                var streamReader = new StreamReader(req.Body);
                                var data = await streamReader.ReadToEndAsync();
                                await resp.WriteAsync($"{routeData.Values["name"]} {data}");
                            }),
                        new ProtoRequestMessage(ProtoMethod.Post, "greeting/James") { Content = new StringContent("Biography") },
                        "James Biography"
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(MatchesRequest))]
        public async Task UseRouter_MapGet_MatchesRequest(Action<IRouteBuilder> routeBuilder, ProtoRequestMessage request, string expected)
        {
            // Arrange
            var gamehostbuilder = new GameHostBuilder();
            gamehostbuilder
                .ConfigureServices(services => services.AddRouting())
                .Configure(app =>
                {
                    app.UseRouter(routeBuilder);
                });
            var testServer = new TestServer(gamehostbuilder);
            var client = testServer.CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(ProtoStatusCode.OK, response.StatusCode);
            var actual = await response.Content.ReadAsStringAsync();
            Assert.Equal(expected, actual);
        }
    }
}
