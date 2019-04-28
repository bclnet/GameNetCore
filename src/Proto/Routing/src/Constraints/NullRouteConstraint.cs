// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Proto;

namespace Contoso.GameNetCore.Routing.Constraints
{
    internal class NullRouteConstraint : IRouteConstraint
    {
        public static readonly NullRouteConstraint Instance = new NullRouteConstraint();

        private NullRouteConstraint()
        {
        }

        public bool Match(ProtoContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            return true;
        }
    }
}
