// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Contoso.GameNetCore.Connections;
using Contoso.GameNetCore.Hosting.Server;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal
{
    internal interface IRequestProcessor
    {
        Task ProcessRequestsAsync<TContext>(IProtoApplication<TContext> application);
        void StopProcessingNextRequest();
        void HandleRequestHeadersTimeout();
        void HandleReadDataRateTimeout();
        void OnInputOrOutputCompleted();
        void Tick(DateTimeOffset now);
        void Abort(ConnectionAbortedException ex);
    }
}