// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Rewrite.Logging;

namespace Contoso.GameNetCore.Rewrite.Internal.UrlActions
{
    public class AbortAction : UrlAction
    {
        public override void ApplyAction(RewriteContext context, BackReferenceCollection ruleBackReferences, BackReferenceCollection conditionBackReferences)
        {
            context.HttpContext.Abort();
            context.Result = RuleResult.EndResponse;
            context.Logger?.AbortedRequest(context.HttpContext.Request.Path + context.HttpContext.Request.QueryString);
        }
    }
}
