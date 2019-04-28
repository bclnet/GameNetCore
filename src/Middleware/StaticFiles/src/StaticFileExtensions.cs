// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using System;

namespace Contoso.GameNetCore.Builder
{
    /// <summary>
    /// Extension methods for the StaticFileMiddleware
    /// </summary>
    public static class StaticFileExtensions
    {
        /// <summary>
        /// Enables static file serving for the current request path
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseStaticFiles(this IApplicationBuilder app) =>
            (app ?? throw new ArgumentNullException(nameof(app))).UseMiddleware<StaticFileMiddleware>();

        /// <summary>
        /// Enables static file serving for the given request path
        /// </summary>
        /// <param name="app"></param>
        /// <param name="requestPath">The relative request path.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseStaticFiles(this IApplicationBuilder app, string requestPath) =>
            (app ?? throw new ArgumentNullException(nameof(app))).UseStaticFiles(new StaticFileOptions
            {
                RequestPath = new PathString(requestPath)
            });

        /// <summary>
        /// Enables static file serving with the given options
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseStaticFiles(this IApplicationBuilder app, StaticFileOptions options) =>
            (app ?? throw new ArgumentNullException(nameof(app))).UseMiddleware<StaticFileMiddleware>(Options.Create(options ?? throw new ArgumentNullException(nameof(options))));
    }
}
