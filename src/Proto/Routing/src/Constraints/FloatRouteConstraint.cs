// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Contoso.GameNetCore.Proto;

namespace Contoso.GameNetCore.Routing.Constraints
{
    /// <summary>
    /// Constrains a route parameter to represent only 32-bit floating-point values.
    /// </summary>
    public class FloatRouteConstraint : IRouteConstraint
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
                if (value is float)
                {
                    return true;
                }

                var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                return float.TryParse(
                    valueString,
                    NumberStyles.Float | NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture,
                    out _);
            }

            return false;
        }
    }
}