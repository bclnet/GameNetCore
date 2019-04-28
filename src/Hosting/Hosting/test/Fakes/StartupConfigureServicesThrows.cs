// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.GameNetCore.Hosting.Fakes
{
    public class StartupConfigureServicesThrows
    {
        public void ConfigureServices(IServiceCollection services)
        {
            throw new Exception("Exception from ConfigureServices");
        }

        public void Configure(IApplicationBuilder builder)
        {

        }
    }
}