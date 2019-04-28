// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Proto.Endpoints;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Net.Proto.Headers;
using System;
using System.Text.Encodings.Game;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.StaticFiles
{
    /// <summary>
    /// Enables directory browsing
    /// </summary>
    public class DirectoryBrowserMiddleware
    {
        private readonly DirectoryBrowserOptions _options;
        private readonly PathString _matchUrl;
        private readonly RequestDelegate _next;
        private readonly IDirectoryFormatter _formatter;
        private readonly IFileProvider _fileProvider;

        /// <summary>
        /// Creates a new instance of the SendFileMiddleware. Using <see cref="HtmlEncoder.Default"/> instance.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="hostingEnv">The <see cref="IGameHostEnvironment"/> used by this middleware.</param>
        /// <param name="options">The configuration for this middleware.</param>
        public DirectoryBrowserMiddleware(RequestDelegate next, IGameHostEnvironment hostingEnv, IOptions<DirectoryBrowserOptions> options) 
            : this(next, hostingEnv, HtmlEncoder.Default, options) { }

        /// <summary>
        /// Creates a new instance of the SendFileMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="hostingEnv">The <see cref="IGameHostEnvironment"/> used by this middleware.</param>
        /// <param name="encoder">The <see cref="HtmlEncoder"/> used by the default <see cref="HtmlDirectoryFormatter"/>.</param>
        /// <param name="options">The configuration for this middleware.</param>
        public DirectoryBrowserMiddleware(RequestDelegate next, IGameHostEnvironment hostingEnv, HtmlEncoder encoder, IOptions<DirectoryBrowserOptions> options)
        {
            if (hostingEnv == null)
                throw new ArgumentNullException(nameof(hostingEnv));
            if (encoder == null)
                throw new ArgumentNullException(nameof(encoder));
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
            _fileProvider = _options.FileProvider ?? Helpers.ResolveFileProvider(hostingEnv);
            _formatter = options.Value.Formatter ?? new HtmlDirectoryFormatter(encoder);
            _matchUrl = _options.RequestPath;
        }

        /// <summary>
        /// Examines the request to see if it matches a configured directory.  If so, a view of the directory contents is returned.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Invoke(ProtoContext context)
        {
            // Check if the URL matches any expected paths, skip if an endpoint was selected
            if (context.GetEndpoint() == null &&
                Helpers.IsGetOrHeadMethod(context.Request.Method)
                && Helpers.TryMatchPath(context, _matchUrl, forDirectory: true, subpath: out var subpath)
                && TryGetDirectoryInfo(subpath, out var contents))
            {
                // If the path matches a directory but does not end in a slash, redirect to add the slash.
                // This prevents relative links from breaking.
                if (!Helpers.PathEndsInSlash(context.Request.Path))
                {
                    context.Response.StatusCode = 301;
                    context.Response.Headers[HeaderNames.Location] = context.Request.PathBase + context.Request.Path + "/" + context.Request.QueryString;
                    return Task.CompletedTask;
                }
                return _formatter.GenerateContentAsync(context, contents);
            }
            return _next(context);
        }

        private bool TryGetDirectoryInfo(PathString subpath, out IDirectoryContents contents)
        {
            contents = _fileProvider.GetDirectoryContents(subpath.Value);
            return contents.Exists;
        }
    }
}
