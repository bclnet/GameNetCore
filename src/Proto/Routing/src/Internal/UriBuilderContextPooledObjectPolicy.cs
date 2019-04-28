// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.Encodings.Game;
using Microsoft.Extensions.ObjectPool;

namespace Contoso.GameNetCore.Routing.Internal
{
    public class UriBuilderContextPooledObjectPolicy : IPooledObjectPolicy<UriBuildingContext>
    {
        public UriBuildingContext Create()
        {
            return new UriBuildingContext(UrlEncoder.Default);
        }

        public bool Return(UriBuildingContext obj)
        {
            obj.Clear();
            return true;
        }
    }
}
