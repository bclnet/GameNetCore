// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Hosting;
using Contoso.GameNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.GameNetCore.StaticFiles
{
    public static class StaticFilesTestServer
    {
        public static TestServer Create(Action<IApplicationBuilder> configureApp, Action<IServiceCollection> configureServices = null)
        {
            Action<IServiceCollection> defaultConfigureServices = services => { };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new []
                {
                    new KeyValuePair<string, string>("webroot", ".")
                })
                .Build();
            var builder = new WebHostBuilder()
                .UseConfiguration(configuration)
                .Configure(configureApp)
                .ConfigureServices(configureServices ?? defaultConfigureServices);
            return new TestServer(builder);
        }
    }
}