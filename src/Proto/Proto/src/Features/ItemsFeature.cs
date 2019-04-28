// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Contoso.GameNetCore.Proto.Internal;

namespace Contoso.GameNetCore.Proto.Features
{
    public class ItemsFeature : IItemsFeature
    {
        public ItemsFeature()
        {
            Items = new ItemsDictionary();
        }

        public IDictionary<object, object> Items { get; set; }
    }
}