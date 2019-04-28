// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.GameUtilities
{
    public class FormReaderAsyncTest : FormReaderTests
    {
        protected override async Task<Dictionary<string, StringValues>> ReadFormAsync(FormReader reader) => await reader.ReadFormAsync();

        protected override async Task<KeyValuePair<string, string>?> ReadPair(FormReader reader) => await reader.ReadNextPairAsync();
    }
}