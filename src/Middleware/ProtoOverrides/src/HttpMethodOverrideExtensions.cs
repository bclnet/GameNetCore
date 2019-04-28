// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.ProtoOverrides;
using Microsoft.Extensions.Options;

namespace Contoso.GameNetCore.Builder
{
    public static class ProtoMethodOverrideExtensions
    {
        /// <summary>
        /// Allows incoming POST request to override method type with type specified in header.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/> instance this method extends.</param>
        public static IApplicationBuilder UseProtoMethodOverride(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<ProtoMethodOverrideMiddleware>();
        }

        /// <summary>
        /// Allows incoming POST request to override method type with type specified in form.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/> instance this method extends.</param>
        /// <param name="options">The <see cref="ProtoMethodOverrideOptions"/>.</param>
        public static IApplicationBuilder UseProtoMethodOverride(this IApplicationBuilder builder, ProtoMethodOverrideOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return builder.UseMiddleware<ProtoMethodOverrideMiddleware>(Options.Create(options));
        }
    }
}
