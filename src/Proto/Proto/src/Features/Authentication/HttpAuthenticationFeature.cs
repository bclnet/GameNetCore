// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;

namespace Contoso.GameNetCore.Proto.Features.Authentication
{
    public class ProtoAuthenticationFeature : IProtoAuthenticationFeature
    {
        public ClaimsPrincipal User
        {
            get;
            set;
        }
    }
}
