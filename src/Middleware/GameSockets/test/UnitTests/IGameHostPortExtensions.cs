// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Contoso.GameNetCore.Hosting.Server.Features;

namespace Contoso.GameNetCore.Hosting
{
    public static class IGameHostPortExtensions
    {
        public static int GetPort(this IGameHost host) => host.GetPorts().First();

        public static IEnumerable<int> GetPorts(this IGameHost host) => host.GetUris().Select(u => u.Port);

        public static IEnumerable<Uri> GetUris(this IGameHost host) => host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.Select(a => new Uri(a));
    }
}