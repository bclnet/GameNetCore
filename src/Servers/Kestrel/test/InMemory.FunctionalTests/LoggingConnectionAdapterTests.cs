// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Contoso.GameNetCore.Hosting;
using Contoso.GameNetCore.Http;
using Contoso.GameNetCore.Server.Kestrel.InMemory.FunctionalTests.TestTransport;
using Contoso.GameNetCore.Testing;
using Contoso.GameNetCore.Testing.xunit;
using Microsoft.Extensions.Logging.Testing;
using Xunit;

namespace Contoso.GameNetCore.Server.Kestrel.InMemory.FunctionalTests
{
    public class LoggingConnectionAdapterTests : LoggedTest
    {
        [Fact]
        [Flaky("https://github.com/aspnet/AspNetCore-Internal/issues/1753", FlakyOn.Helix.All)]
        public async Task LoggingConnectionAdapterCanBeAddedBeforeAndAfterHttpsAdapter()
        {
            await using (var server = new TestServer(context =>
                {
                    context.Response.ContentLength = 12;
                    return context.Response.WriteAsync("Hello World!");
                },
                new TestServiceContext(LoggerFactory),
                listenOptions =>
                {
                    listenOptions.UseConnectionLogging();
                    listenOptions.UseHttps(TestResources.GetTestCertificate());
                    listenOptions.UseConnectionLogging();
                }))
            {
                {
                    var response = await server.HttpClientSlim.GetStringAsync($"https://localhost:{server.Port}/", validateCertificate: false)
                                                              .DefaultTimeout();
                    Assert.Equal("Hello World!", response);
                }
            }
        }
    }
}
