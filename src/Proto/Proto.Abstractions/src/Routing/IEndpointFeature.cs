// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Proto.Features
{
    /// <summary>
    /// A feature interface for endpoint routing. Use <see cref="ProtoContext.Features"/>
    /// to access an instance associated with the current request.
    /// </summary>
    public interface IEndpointFeature
    {
        /// <summary>
        /// Gets or sets the selected <see cref="Proto.Endpoint"/> for the current
        /// request.
        /// </summary>
        Endpoint Endpoint { get; set; }
    }
}
