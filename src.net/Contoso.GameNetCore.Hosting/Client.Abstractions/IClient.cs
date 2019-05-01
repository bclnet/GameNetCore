// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETX
using Microsoft.AspNetCore.Http.Features;
#else
using Contoso.GameNetCore.Proto.Features;
#endif
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.Hosting.Client
{
    /// <summary>
    /// Represents a client.
    /// </summary>
    public interface IClient : IDisposable
    {
        /// <summary>
        /// A collection of PROTO features of the client.
        /// </summary>
        IFeatureCollection Features { get; }

        /// <summary>
        /// Start the client with an application.
        /// </summary>
        /// <param name="application">An instance of <see cref="IProtoApplication{TContext}"/>.</param>
        /// <typeparam name="TContext">The context associated with the application.</typeparam>
        /// <param name="cancellationToken">Indicates if the client startup should be aborted.</param>
        Task StartAsync<TContext>(IProtoApplication<TContext> application, CancellationToken cancellationToken);

        /// <summary>
        /// Stop processing requests and shut down the client, gracefully if possible.
        /// </summary>
        /// <param name="cancellationToken">Indicates if the graceful shutdown should be aborted.</param>
        Task StopAsync(CancellationToken cancellationToken);
    }
}
