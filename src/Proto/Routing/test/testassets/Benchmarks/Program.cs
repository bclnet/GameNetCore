// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            GetGameHostBuilder(args).Build().Run();
        }

        public static IGameHostBuilder GetGameHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .AddEnvironmentVariables(prefix: "RoutingBenchmarks_")
                .Build();

            // Consoler logger has a major impact on perf results, so do not use
            // default builder.

            var gameHostBuilder = new GameHostBuilder()
                    .UseConfiguration(config)
                    .UseKestrel();

            var scenario = config["scenarios"]?.ToLower();
            if (scenario == "plaintextdispatcher" || scenario == "plaintextendpointrouting")
            {
                gameHostBuilder.UseStartup<StartupUsingEndpointRouting>();
                // for testing
                gameHostBuilder.UseSetting("Startup", nameof(StartupUsingEndpointRouting));
            }
            else if (scenario == "plaintextrouting" || scenario == "plaintextrouter")
            {
                gameHostBuilder.UseStartup<StartupUsingRouter>();
                // for testing
                gameHostBuilder.UseSetting("Startup", nameof(StartupUsingRouter));
            }
            else
            {
                throw new InvalidOperationException(
                    $"Invalid scenario '{scenario}'. Allowed scenarios are PlaintextEndpointRouting and PlaintextRouter");
            }

            return gameHostBuilder;
        }
    }
}
