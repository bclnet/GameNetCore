// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Proto.Features
{
    /// <summary>
    /// Configures response compression behavior for HTTPS on a per-request basis.
    /// </summary>
    public interface IProtosCompressionFeature
    {
        /// <summary>
        /// The <see cref="ProtosCompressionMode"/> to use.
        /// </summary>
        ProtosCompressionMode Mode { get; set; }
    }
}
