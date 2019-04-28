// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace Contoso.GameNetCore.Hosting
{
    public static class GameHostBuilderClientExtensions
    {
        public static IGameHostBuilder UseClient(this IGameHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(services =>
            {
            });
        }
    }
}
