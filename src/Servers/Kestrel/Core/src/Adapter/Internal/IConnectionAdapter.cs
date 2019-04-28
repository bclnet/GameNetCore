// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Adapter.Internal
{
    public interface IConnectionAdapter
    {
        bool IsProtos { get; }
        Task<IAdaptedConnection> OnConnectionAsync(ConnectionAdapterContext context);
    }
}
