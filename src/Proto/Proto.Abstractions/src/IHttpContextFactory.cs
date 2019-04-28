// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Proto.Features;

namespace Contoso.GameNetCore.Proto
{
    public interface IProtoContextFactory
    {
        ProtoContext Create(IFeatureCollection featureCollection);
        void Dispose(ProtoContext httpContext);
    }
}
