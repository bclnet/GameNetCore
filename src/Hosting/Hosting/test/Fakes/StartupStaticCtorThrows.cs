// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Builder;

namespace Contoso.GameNetCore.Hosting.Fakes
{
    public class StartupStaticCtorThrows
    {
        static StartupStaticCtorThrows()
        {
            throw new Exception("Exception from static constructor");
        }

        public void Configure(IApplicationBuilder app)
        {
        }
    }
}