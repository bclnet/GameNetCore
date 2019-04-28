﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Proto.Features;

namespace Contoso.GameNetCore.Proto.Endpoints
{
    /// <summary>
    /// Extension methods to expose Endpoint on ProtoContext.
    /// </summary>
    public static class EndpointProtoContextExtensions
    {
        /// <summary>
        /// Extension method for getting the <see cref="Endpoint"/> for the current request.
        /// </summary>
        /// <param name="context">The <see cref="ProtoContext"/> context.</param>
        /// <returns>The <see cref="Endpoint"/>.</returns>
        public static Endpoint GetEndpoint(this ProtoContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.Features.Get<IEndpointFeature>()?.Endpoint;
        }

        /// <summary>
        /// Extension method for setting the <see cref="Endpoint"/> for the current request.
        /// </summary>
        /// <param name="context">The <see cref="ProtoContext"/> context.</param>
        /// <param name="endpoint">The <see cref="Endpoint"/>.</param>
        public static void SetEndpoint(this ProtoContext context, Endpoint endpoint)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var feature = context.Features.Get<IEndpointFeature>();

            if (endpoint != null)
            {
                if (feature == null)
                {
                    feature = new EndpointFeature();
                    context.Features.Set(feature);
                }

                feature.Endpoint = endpoint;
            }
            else
            {
                if (feature == null)
                {
                    // No endpoint to set and no feature on context. Do nothing
                    return;
                }

                feature.Endpoint = null;
            }
        }

        private class EndpointFeature : IEndpointFeature
        {
            public Endpoint Endpoint { get; set; }
        }
    }
}
