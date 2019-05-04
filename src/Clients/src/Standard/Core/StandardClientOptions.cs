// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Contoso.GameNetCore.Client.Standard.Core
{
    /// <summary>
    /// Provides programmatic configuration of Simple-specific features.
    /// </summary>
    public class StandardClientOptions
    {
        /// <summary>
        /// Enables the Listen options callback to resolve and use services registered by the application during startup.
        /// Typically initialized by UseStandard()"/>.
        /// </summary>
        public IServiceProvider ApplicationServices { get; set; }

        /// <summary>
        /// Provides a configuration source where endpoints will be loaded from on client start.
        /// The default is null.
        /// </summary>
        public StandardConfigurationLoader ConfigurationLoader { get; set; }
    }
}
