// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Contoso.GameNetCore.Proto;

namespace Contoso.GameNetCore.Routing
{
    public static class TestConstants
    {
        internal static readonly RequestDelegate EmptyRequestDelegate = (context) => Task.CompletedTask;
    }
}
