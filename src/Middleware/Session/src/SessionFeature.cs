// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Http;
using Contoso.GameNetCore.Http.Features;

namespace Contoso.GameNetCore.Session
{
    public class SessionFeature : ISessionFeature
    {
        public ISession Session { get; set; }
    }
}