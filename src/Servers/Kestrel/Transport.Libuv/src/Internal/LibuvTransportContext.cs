// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Microsoft.Extensions.Hosting;

namespace Contoso.GameNetCore.Server.Kestrel.Transport.Libuv.Internal
{
    internal class LibuvTransportContext
    {
        public LibuvTransportOptions Options { get; set; }

        public IHostApplicationLifetime AppLifetime { get; set; }

        public ILibuvTrace Log { get; set; }

        public IConnectionDispatcher ConnectionDispatcher { get; set; }
    }
}
