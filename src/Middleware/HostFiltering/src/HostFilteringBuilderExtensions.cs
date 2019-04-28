﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.HostFiltering;
#if NETX
using Microsoft.AspNetCore.Builder;
#else
using Contoso.GameNetCore.Builder;
#endif
using System;

namespace Contoso.GameNetCore.Builder
{
    /// <summary>
    /// Extension methods for the HostFiltering middleware.
    /// </summary>
    public static class HostFilteringBuilderExtensions
    {
        /// <summary>
        /// Adds middleware for filtering requests by allowed host headers. Invalid requests will be rejected with a
        /// 400 status code.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance this method extends.</param>
        /// <returns>The original <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseHostFiltering(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            app.UseMiddleware<HostFilteringMiddleware>();
            return app;
        }
    }
}
