// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Proto.Features
{
    /// <summary>
    /// Controls the IO behavior for the <see cref="IProtoRequestFeature.Body"/> and <see cref="IProtoResponseFeature.Body"/> 
    /// </summary>
    public interface IProtoBodyControlFeature
    {
        /// <summary>
        /// Gets or sets a value that controls whether synchronous IO is allowed for the <see cref="IProtoRequestFeature.Body"/> and <see cref="IProtoResponseFeature.Body"/> 
        /// </summary>
        bool AllowSynchronousIO { get; set; }
    }
}
