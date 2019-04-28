// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Proto;
using Microsoft.Extensions.FileProviders;
using System;

namespace Contoso.GameNetCore.StaticFiles.Infrastructure
{
    /// <summary>
    /// Options common to several middleware components
    /// </summary>
    public class SharedOptions
    {
        private PathString _requestPath;

        /// <summary>
        /// Defaults to all request paths.
        /// </summary>
        public SharedOptions() => RequestPath = PathString.Empty;

        /// <summary>
        /// The request path that maps to static resources
        /// </summary>
        public PathString RequestPath
        {
            get => _requestPath;
            set
            {
                if (value.HasValue && value.Value.EndsWith("/", StringComparison.Ordinal))
                    throw new ArgumentException("Request path must not end in a slash");
                _requestPath = value;
            }
        }

        /// <summary>
        /// The file system used to locate resources
        /// </summary>
        public IFileProvider FileProvider { get; set; }
    }
}
