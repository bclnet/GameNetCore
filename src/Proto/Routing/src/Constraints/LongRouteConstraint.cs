// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Contoso.GameNetCore.Proto;

namespace Contoso.GameNetCore.Routing.Constraints
{
    /// <summary>
    /// Constrains a route parameter to represent only 64-bit integer values.
    /// </summary>
    public class LongRouteConstraint : IRouteConstraint
    {
        /// <inheritdoc />
        public bool Match(
            ProtoContext httpContext,
            IRouter route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (routeKey == null)
            {
                throw new ArgumentNullException(nameof(routeKey));
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.TryGetValue(routeKey, out var value) && value != null)
            {
                if (value is long)
                {
                    return true;
                }

                var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                return long.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
            }

            return false;
        }
    }
}