// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Builder;

namespace Contoso.GameNetCore.Hosting.Fakes
{
    public class StartupTwoConfigures
    {
        public StartupTwoConfigures()
        {
        }

        public void Configure(IApplicationBuilder builder)
        {

        }

        public void Configure(IApplicationBuilder builder, object service)
        {

        }
    }
}