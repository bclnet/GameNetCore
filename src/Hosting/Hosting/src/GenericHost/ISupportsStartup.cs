// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETX
using Microsoft.AspNetCore.Builder;
#else
using Contoso.GameNetCore.Builder;
#endif
using System;

namespace Contoso.GameNetCore.Hosting.Internal
{
    internal interface ISupportsStartup
    {
        IGameHostBuilder Configure(Action<GameHostBuilderContext, IApplicationBuilder> configure);
        IGameHostBuilder UseStartup(Type startupType);
    }
}
