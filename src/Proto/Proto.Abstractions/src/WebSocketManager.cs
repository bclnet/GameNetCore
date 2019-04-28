// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.GameSockets;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.Proto
{
    /// <summary>
    /// Manages the establishment of GameSocket connections for a specific HTTP request. 
    /// </summary>
    public abstract class GameSocketManager
    {
        /// <summary>
        /// Gets a value indicating whether the request is a GameSocket establishment request.
        /// </summary>
        public abstract bool IsGameSocketRequest { get; }

        /// <summary>
        /// Gets the list of requested GameSocket sub-protocols.
        /// </summary>
        public abstract IList<string> GameSocketRequestedProtocols { get; }

        /// <summary>
        /// Transitions the request to a GameSocket connection.
        /// </summary>
        /// <returns>A task representing the completion of the transition.</returns>
        public virtual Task<GameSocket> AcceptGameSocketAsync()
        {
            return AcceptGameSocketAsync(subProtocol: null);
        }

        /// <summary>
        /// Transitions the request to a GameSocket connection using the specified sub-protocol.
        /// </summary>
        /// <param name="subProtocol">The sub-protocol to use.</param>
        /// <returns>A task representing the completion of the transition.</returns>
        public abstract Task<GameSocket> AcceptGameSocketAsync(string subProtocol);
    }
}
