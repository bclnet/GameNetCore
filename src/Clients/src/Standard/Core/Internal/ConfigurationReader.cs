// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Configuration;
using System;

namespace Contoso.GameNetCore.Client.Standard.Core.Internal
{
    internal class ConfigurationReader
    {
        IConfiguration _configuration;

        public ConfigurationReader(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
    }
}
