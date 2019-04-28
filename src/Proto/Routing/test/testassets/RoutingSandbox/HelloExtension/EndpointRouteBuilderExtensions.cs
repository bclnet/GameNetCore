// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contoso.GameNetCore.Routing;
using Contoso.GameNetCore.Routing.Matching;
using Contoso.GameNetCore.Routing.Patterns;

namespace Contoso.GameNetCore.Builder
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapHello(this IEndpointRouteBuilder endpoints, string pattern, string greeter)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            var pipeline = endpoints.CreateApplicationBuilder()
               .UseHello(greeter)
               .Build();

            return endpoints.Map(pattern, pipeline);
        }
    }
}
