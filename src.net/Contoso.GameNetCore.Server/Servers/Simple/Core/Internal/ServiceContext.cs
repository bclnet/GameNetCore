// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Server.Simple.Core.Internal.Infrastructure;

namespace Contoso.GameNetCore.Server.Simple.Core.Internal
{
    internal class ServiceContext
    {
        public ISimpleTrace Log { get; set; }

        public SimpleServerOptions ServerOptions { get; set; }
    }
}
