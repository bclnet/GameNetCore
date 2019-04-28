// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Http;
using Contoso.GameNetCore.Rewrite.Internal.UrlActions;
using Xunit;

namespace Contoso.GameNetCore.Rewrite.Tests.UrlActions
{
    public class AbortActionTests
    {
        public void AbortAction_VerifyEndResponseResult()
        {
            var context = new RewriteContext { HttpContext = new DefaultHttpContext() };
            var action = new AbortAction();

            action.ApplyAction(context, null, null);

            Assert.Equal(RuleResult.EndResponse, context.Result);
        }
    }
}
