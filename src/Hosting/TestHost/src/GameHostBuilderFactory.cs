// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Contoso.GameNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Contoso.GameNetCore.TestHost
{
    public static class GameHostBuilderFactory
    {
        public static IGameHostBuilder CreateFromAssemblyEntryPoint(Assembly assembly, string[] args)
        {
            var factory = HostFactoryResolver.ResolveGameHostBuilderFactory<IGameHostBuilder>(assembly);
            return factory?.Invoke(args);
        }

        public static IGameHostBuilder CreateFromTypesAssemblyEntryPoint<T>(string[] args) =>
            CreateFromAssemblyEntryPoint(typeof(T).Assembly, args);
    }
}
