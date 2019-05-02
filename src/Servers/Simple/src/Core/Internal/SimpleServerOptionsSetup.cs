// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Options;

namespace Contoso.GameNetCore.Server.Simple.Core.Internal
{
    internal class SimpleServerOptionsSetup : IConfigureOptions<SimpleServerOptions>
    {
        IServiceProvider _services;

        public SimpleServerOptionsSetup(IServiceProvider services) => _services = services;

        public void Configure(SimpleServerOptions options) => options.ApplicationServices = _services;
    }
}
