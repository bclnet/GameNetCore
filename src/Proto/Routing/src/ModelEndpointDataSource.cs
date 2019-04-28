// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Proto;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Contoso.GameNetCore.Routing
{
    internal class ModelEndpointDataSource : EndpointDataSource
    {
        private List<DefaultEndpointConventionBuilder> _endpointConventionBuilders;

        public ModelEndpointDataSource()
        {
            _endpointConventionBuilders = new List<DefaultEndpointConventionBuilder>();
        }

        public IEndpointConventionBuilder AddEndpointBuilder(EndpointBuilder endpointBuilder)
        {
            var builder = new DefaultEndpointConventionBuilder(endpointBuilder);
            _endpointConventionBuilders.Add(builder);

            return builder;
        }

        public override IChangeToken GetChangeToken()
        {
            return NullChangeToken.Singleton;
        }

        public override IReadOnlyList<Endpoint> Endpoints => _endpointConventionBuilders.Select(e => e.Build()).ToArray();

        // for testing
        internal IEnumerable<EndpointBuilder> EndpointBuilders => _endpointConventionBuilders.Select(b => b.EndpointBuilder);
    }
}
