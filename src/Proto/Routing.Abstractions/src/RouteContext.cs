// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Proto;

namespace Contoso.GameNetCore.Routing
{
    /// <summary>
    /// A context object for <see cref="IRouter.RouteAsync(RouteContext)"/>.
    /// </summary>
    public class RouteContext
    {
        private RouteData _routeData;

        /// <summary>
        /// Creates a new instance of <see cref="RouteContext"/> for the provided <paramref name="httpContext"/>.
        /// </summary>
        /// <param name="httpContext">The <see cref="Proto.ProtoContext"/> associated with the current request.</param>
        public RouteContext(ProtoContext httpContext)
        {
            ProtoContext = httpContext;

            RouteData = new RouteData();
        }

        /// <summary>
        /// Gets or sets the handler for the request. An <see cref="IRouter"/> should set <see cref="Handler"/>
        /// when it matches.
        /// </summary>
        public RequestDelegate Handler { get; set; }

        /// <summary>
        /// Gets the <see cref="Proto.ProtoContext"/> associated with the current request.
        /// </summary>
        public ProtoContext ProtoContext { get; }

        /// <summary>
        /// Gets or sets the <see cref="Routing.RouteData"/> associated with the current context.
        /// </summary>
        public RouteData RouteData
        {
            get
            {
                return _routeData;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(RouteData));
                }

                _routeData = value;
            }
        }
    }
}
