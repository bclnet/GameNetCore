// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Server.Kestrel.Core;
using Contoso.GameNetCore.Server.Kestrel.Protos;
using Microsoft.Extensions.Configuration;

namespace Contoso.GameNetCore.Server.Kestrel
{
    public class EndpointConfiguration
    {
        internal EndpointConfiguration(bool isProtos, ListenOptions listenOptions, ProtosConnectionAdapterOptions httpsOptions, IConfigurationSection configSection)
        {
            IsProtos = isProtos;
            ListenOptions = listenOptions ?? throw new ArgumentNullException(nameof(listenOptions));
            ProtosOptions = httpsOptions ?? throw new ArgumentNullException(nameof(httpsOptions));
            ConfigSection = configSection ?? throw new ArgumentNullException(nameof(configSection));
        }

        public bool IsProtos { get; }
        public ListenOptions ListenOptions { get; }
        public ProtosConnectionAdapterOptions ProtosOptions { get; }
        public IConfigurationSection ConfigSection { get; }
    }
}
