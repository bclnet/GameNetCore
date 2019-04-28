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
    public abstract class SharedOptionsBase
    {
        /// <summary>
        /// Creates an new instance of the SharedOptionsBase.
        /// </summary>
        /// <param name="sharedOptions"></param>
        protected SharedOptionsBase(SharedOptions sharedOptions) =>
            SharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));

        /// <summary>
        /// Options common to several middleware components
        /// </summary>
        protected SharedOptions SharedOptions { get; private set; }

        /// <summary>
        /// The relative request path that maps to static resources.
        /// </summary>
        public PathString RequestPath
        {
            get => SharedOptions.RequestPath;
            set => SharedOptions.RequestPath = value;
        }

        /// <summary>
        /// The file system used to locate resources
        /// </summary>
        public IFileProvider FileProvider
        {
            get => SharedOptions.FileProvider;
            set => SharedOptions.FileProvider = value;
        }
    }
}
