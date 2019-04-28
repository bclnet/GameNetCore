// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Contoso.GameNetCore.Hosting.Fakes
{
    public class StartupWithILoggerFactory
    {
        public ILoggerFactory ConstructorLoggerFactory { get; set; }

        public ILoggerFactory ConfigureLoggerFactory { get; set; }

        public StartupWithILoggerFactory(ILoggerFactory constructorLoggerFactory)
        {
             ConstructorLoggerFactory = constructorLoggerFactory;
        }

        public void ConfigureServices(IServiceCollection collection)
        {
            collection.AddSingleton(this);
        }

        public void Configure(IApplicationBuilder builder, ILoggerFactory loggerFactory)
        {
            ConfigureLoggerFactory = loggerFactory;
        }
    }
}