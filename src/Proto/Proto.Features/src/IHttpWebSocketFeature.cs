// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.GameSockets;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.Proto.Features
{
    public interface IProtoGameSocketFeature
    {
        /// <summary>
        /// Indicates if this is a GameSocket upgrade request.
        /// </summary>
        bool IsGameSocketRequest { get; }

        /// <summary>
        /// Attempts to upgrade the request to a <see cref="GameSocket"/>. Check <see cref="IsGameSocketRequest"/>
        /// before invoking this.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<GameSocket> AcceptAsync(GameSocketAcceptContext context);
    }
}