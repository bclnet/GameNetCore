// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Options;

namespace Contoso.GameNetCore.Client.Standard.Core.Internal
{
    internal class StandardClientOptionsSetup : IConfigureOptions<StandardClientOptions>
    {
        IServiceProvider _services;

        public StandardClientOptionsSetup(IServiceProvider services) => _services = services;

        public void Configure(StandardClientOptions options) => options.ApplicationServices = _services;
    }
}
