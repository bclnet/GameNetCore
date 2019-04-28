// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Hosting;
using Contoso.GameNetCore.Proto;
using Microsoft.Extensions.FileProviders;
using System;

namespace Contoso.GameNetCore.StaticFiles
{
    internal static class Helpers
    {
        internal static IFileProvider ResolveFileProvider(IGameHostEnvironment hostingEnv) =>
            hostingEnv.GameRootFileProvider ?? throw new InvalidOperationException("Missing FileProvider.");

        internal static bool IsGetOrHeadMethod(string method) => ProtoMethods.IsGet(method) || ProtoMethods.IsHead(method);

        internal static bool PathEndsInSlash(PathString path) => path.Value.EndsWith("/", StringComparison.Ordinal);

        internal static bool TryMatchPath(ProtoContext context, PathString matchUrl, bool forDirectory, out PathString subpath)
        {
            var path = context.Request.Path;
            if (forDirectory && !PathEndsInSlash(path))
                path += new PathString("/");
            return path.StartsWithSegments(matchUrl, out subpath);
        }
    }
}
