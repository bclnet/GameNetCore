// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Hosting;
using Contoso.GameNetCore.Http;
using Contoso.GameNetCore.Server.Kestrel.Core;
using Contoso.GameNetCore.Testing;
using Contoso.GameNetCore.Testing.xunit;
using Microsoft.Extensions.Logging.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Interop.FunctionalTests
{
    [OSSkipCondition(OperatingSystems.MacOSX, SkipReason = "Missing SslStream ALPN support: https://github.com/dotnet/corefx/issues/30492")]
    [MinimumOSVersion(OperatingSystems.Windows, WindowsVersions.Win81,
        SkipReason = "Missing Windows ALPN support: https://en.wikipedia.org/wiki/Application-Layer_Protocol_Negotiation#Support")]
    public class H2SpecTests : LoggedTest
    {
        [ConditionalTheory]
        [MemberData(nameof(H2SpecTestCases))]
        [SkipOnHelix] // https://github.com/aspnet/AspNetCore/issues/7299
        public async Task RunIndividualTestCase(H2SpecTestCase testCase)
        {
            var hostBuilder = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, 0, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http2;
                        if (testCase.Https)
                        {
                            listenOptions.UseHttps(TestResources.GetTestCertificate());
                        }
                    });
                })
                .ConfigureServices(AddTestLogging)
                .Configure(ConfigureHelloWorld);

            using (var host = hostBuilder.Build())
            {
                await host.StartAsync();

                await H2SpecCommands.RunTest(testCase.Id, host.GetPort(), testCase.Https, Logger);

                await host.StopAsync();
            }
        }

        public static TheoryData<H2SpecTestCase> H2SpecTestCases
        {
            get
            {
                var dataset = new TheoryData<H2SpecTestCase>();
                var toSkip = new string[] { /*"http2/5.1/8"*/ };

                foreach (var testcase in H2SpecCommands.EnumerateTestCases())
                {
                    string skip = null;
                    if (toSkip.Contains(testcase.Item1))
                    {
                        skip = "https://github.com/aspnet/KestrelHttpServer/issues/2154";
                    }

                    dataset.Add(new H2SpecTestCase
                    {
                        Id = testcase.Item1,
                        Description = testcase.Item2,
                        Https = false,
                        Skip = skip,
                    });

                    dataset.Add(new H2SpecTestCase
                    {
                        Id = testcase.Item1,
                        Description = testcase.Item2,
                        Https = true,
                        Skip = skip,
                    });
                }

                return dataset;
            }
        }

        public class H2SpecTestCase : IXunitSerializable
        {
            // For the serializer
            public H2SpecTestCase()
            {
            }

            public string Id { get; set; }
            public string Description { get; set; }
            public bool Https { get; set; }
            public string Skip { get; set; }

            public void Deserialize(IXunitSerializationInfo info)
            {
                Id = info.GetValue<string>(nameof(Id));
                Description = info.GetValue<string>(nameof(Description));
                Https = info.GetValue<bool>(nameof(Https));
                Skip = info.GetValue<string>(nameof(Skip));
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(Id), Id, typeof(string));
                info.AddValue(nameof(Description), Description, typeof(string));
                info.AddValue(nameof(Https), Https, typeof(bool));
                info.AddValue(nameof(Skip), Skip, typeof(string));
            }

            public override string ToString()
            {
                return $"{Id}, HTTPS:{Https}, {Description}";
            }
        }

        private void ConfigureHelloWorld(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                // Read the whole request body to check for errors.
                await context.Request.Body.CopyToAsync(Stream.Null);
                await context.Response.WriteAsync("Hello World");
            });
        }
    }
}
