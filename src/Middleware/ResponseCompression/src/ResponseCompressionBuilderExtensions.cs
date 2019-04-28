// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Contoso.GameNetCore.Builder
{
    /// <summary>
    /// Extension methods for the ResponseCompression middleware.
    /// </summary>
    public static class ResponseCompressionBuilderExtensions
    {
        /// <summary>
        /// Adds middleware for dynamically compressing HTTP Responses.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/> instance this method extends.</param>
        public static IApplicationBuilder UseResponseCompression(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<ResponseCompressionMiddleware>();
        }
    }
}
