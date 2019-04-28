// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Builder.Internal;
using Contoso.GameNetCore.Proto.Features;
using System;

namespace Contoso.GameNetCore.Hosting.Builder
{
    public class ApplicationBuilderFactory : IApplicationBuilderFactory
    {
        readonly IServiceProvider _serviceProvider;

        public ApplicationBuilderFactory(IServiceProvider serviceProvider) =>
            _serviceProvider = serviceProvider;

        public IApplicationBuilder CreateBuilder(IFeatureCollection serverFeatures) =>
            new ApplicationBuilder(_serviceProvider, serverFeatures);
    }
}
