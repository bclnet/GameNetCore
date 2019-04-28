// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Proto;
using Microsoft.Extensions.FileProviders;
using System;

namespace Contoso.GameNetCore.StaticFiles
{
    /// <summary>
    /// Contains information about the request and the file that will be served in response.
    /// </summary>
    public class StaticFileResponseContext
    {
        /// <summary>
        /// Constructs the <see cref="StaticFileResponseContext"/>.
        /// </summary>
        [Obsolete("Use the constructor that passes in the ProtoContext and IFileInfo parameters: StaticFileResponseContext(ProtoContext context, IFileInfo file)", false)]
        public StaticFileResponseContext() { }

        /// <summary>
        /// Constructs the <see cref="StaticFileResponseContext"/>.
        /// </summary>
        /// <param name="context">The request and response information.</param>
        /// <param name="file">The file to be served.</param>
        public StaticFileResponseContext(ProtoContext context, IFileInfo file)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            File = file ?? throw new ArgumentNullException(nameof(file));
        }

        /// <summary>
        /// The request and response information.
        /// </summary>
        public ProtoContext Context { get; }

        /// <summary>
        /// The file to be served.
        /// </summary>
        public IFileInfo File { get; }
    }
}
