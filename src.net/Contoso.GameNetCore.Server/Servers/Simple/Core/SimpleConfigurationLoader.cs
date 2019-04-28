// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Server.Simple.Core;
using Contoso.GameNetCore.Server.Simple.Core.Internal;
using Microsoft.Extensions.Configuration;
using System;

namespace Contoso.GameNetCore.Server.Simple
{
    public class SimpleConfigurationLoader
    {
        bool _loaded = false;

        internal SimpleConfigurationLoader(SimpleServerOptions options, IConfiguration configuration)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            ConfigurationReader = new ConfigurationReader(Configuration);
        }

        public SimpleServerOptions Options { get; }
        public IConfiguration Configuration { get; }
        internal ConfigurationReader ConfigurationReader { get; }

        // Called from ApplyEndpointDefaults so it applies to even explicit Listen endpoints.
        // Does not require a call to Load.
        internal void ApplyConfigurationDefaults(ListenOptions listenOptions)
        {
        }

        public void Load()
        {
            if (_loaded)
                // The loader has already been run.
                return;
            _loaded = true;
        }
    }
}
