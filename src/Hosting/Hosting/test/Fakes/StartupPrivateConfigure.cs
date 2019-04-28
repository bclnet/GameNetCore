// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.GameNetCore.Hosting.Fakes
{
    public class StartupPrivateConfigure
    {
        public StartupPrivateConfigure()
        {
        }
        
        public void ConfigureServices(IServiceCollection services)
        {

        }

        private void Configure(IApplicationBuilder builder)
        {

        }
    }
}