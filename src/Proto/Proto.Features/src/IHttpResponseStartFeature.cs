// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.Proto.Features
{
    /// <summary>
    /// Feature to start response writing.
    /// </summary>
    public interface IProtoResponseStartFeature
    {
        /// <summary>
        /// Starts the response by calling OnStarting() and making headers unmodifiable.
        /// </summary>
        Task StartAsync(CancellationToken token = default);
    }
}
