// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Contoso.GameNetCore.GameSockets
{
    public static class GameSocketsDependencyInjectionExtensions
    {
        public static IServiceCollection AddGameSockets(this IServiceCollection services, Action<GameSocketOptions> configure) =>
            services.Configure(configure);
    }
}
