// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Proto;

namespace Contoso.GameNetCore.Routing
{
    /// <summary>
    /// A context for virtual path generation operations.
    /// </summary>
    public class VirtualPathContext
    {
        /// <summary>
        /// Creates a new instance of <see cref="VirtualPathContext"/>.
        /// </summary>
        /// <param name="httpContext">The <see cref="Proto.ProtoContext"/> associated with the current request.</param>
        /// <param name="ambientValues">The set of route values associated with the current request.</param>
        /// <param name="values">The set of new values provided for virtual path generation.</param>
        public VirtualPathContext(
            ProtoContext httpContext,
            RouteValueDictionary ambientValues,
            RouteValueDictionary values)
            : this(httpContext, ambientValues, values, null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="VirtualPathContext"/>.
        /// </summary>
        /// <param name="httpContext">The <see cref="Proto.ProtoContext"/> associated with the current request.</param>
        /// <param name="ambientValues">The set of route values associated with the current request.</param>
        /// <param name="values">The set of new values provided for virtual path generation.</param>
        /// <param name="routeName">The name of the route to use for virtual path generation.</param>
        public VirtualPathContext(
            ProtoContext httpContext,
            RouteValueDictionary ambientValues,
            RouteValueDictionary values,
            string routeName)
        {
            ProtoContext = httpContext;
            AmbientValues = ambientValues;
            Values = values;
            RouteName = routeName;
        }

        /// <summary>
        /// Gets the set of route values associated with the current request.
        /// </summary>
        public RouteValueDictionary AmbientValues { get; }

        /// <summary>
        /// Gets the <see cref="Proto.ProtoContext"/> associated with the current request.
        /// </summary>
        public ProtoContext ProtoContext { get; }

        /// <summary>
        /// Gets the name of the route to use for virtual path generation.
        /// </summary>
        public string RouteName { get; }

        /// <summary>
        /// Gets or sets the set of new values provided for virtual path generation.
        /// </summary>
        public RouteValueDictionary Values { get; set; }
    }
}
