// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Testing.xunit;

namespace Contoso.GameNetCore.Server.IntegrationTesting
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Assembly | AttributeTargets.Class)]
    public sealed partial class SkipIfIISExpressSchemaMissingInProcessAttribute : Attribute, ITestCondition
    {
        public bool IsMet => IISExpressAncmSchema.SupportsInProcessHosting;

        public string SkipReason => IISExpressAncmSchema.SkipReason;
    }
}
