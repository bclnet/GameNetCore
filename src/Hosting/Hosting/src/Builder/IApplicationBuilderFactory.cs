// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETX
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
#else
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Proto.Features;
#endif

namespace Contoso.GameNetCore.Hosting.Builder
{
    public interface IApplicationBuilderFactory
    {
        IApplicationBuilder CreateBuilder(IFeatureCollection serverFeatures);
    }
}