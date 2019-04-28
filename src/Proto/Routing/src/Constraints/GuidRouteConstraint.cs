// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Contoso.GameNetCore.Proto;

namespace Contoso.GameNetCore.Routing.Constraints
{
    /// <summary>
    /// Constrains a route parameter to represent only <see cref="Guid"/> values.
    /// Matches values specified in any of the five formats "N", "D", "B", "P", or "X",
    /// supported by Guid.ToString(string) and Guid.ToString(String, IFormatProvider) methods.
    /// </summary>
    public class GuidRouteConstraint : IRouteConstraint
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
                if (value is Guid)
                {
                    return true;
                }

                var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                return Guid.TryParse(valueString, out _);
            }

            return false;
        }
    }
}