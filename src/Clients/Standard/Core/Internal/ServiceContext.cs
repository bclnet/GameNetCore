// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Client.Standard.Core.Internal.Infrastructure;

namespace Contoso.GameNetCore.Client.Standard.Core.Internal
{
    internal class ServiceContext
    {
        public IStandardTrace Log { get; set; }

        public StandardClientOptions ClientOptions { get; set; }
    }
}
