// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETX
using Microsoft.AspNetCore.Builder;
#else
using Contoso.GameNetCore.Builder;
#endif
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Contoso.GameNetCore.Hosting
{
    public interface IStartup
    {
        IServiceProvider ConfigureServices(IServiceCollection services);

        void Configure(IApplicationBuilder app);
    }
}