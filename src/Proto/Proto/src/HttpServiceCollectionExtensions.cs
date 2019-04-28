// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Proto;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring ProtoContext services.
    /// </summary>
    public static class ProtoServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a default implementation for the <see cref="IProtoContextAccessor"/> service.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddProtoContextAccessor(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IProtoContextAccessor, ProtoContextAccessor>();
            return services;
        }
    }
}
