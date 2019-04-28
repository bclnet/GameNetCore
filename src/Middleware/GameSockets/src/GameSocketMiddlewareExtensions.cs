// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.GameSockets;
using Microsoft.Extensions.Options;
using System;

namespace Contoso.GameNetCore.Builder
{
    public static class GameSocketMiddlewareExtensions
    {
        public static IApplicationBuilder UseWebSockets(this IApplicationBuilder app) =>
            (app ?? throw new ArgumentNullException(nameof(app))).UseMiddleware<GameSocketMiddleware>();

        public static IApplicationBuilder UseWebSockets(this IApplicationBuilder app, GameSocketOptions options) =>
            (app ?? throw new ArgumentNullException(nameof(app))).UseMiddleware<GameSocketMiddleware>(Options.Create(options ?? throw new ArgumentNullException(nameof(options))));
    }
}