// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.AspNetCore.Http
{
    public class DefaultHttpContextFactory : IHttpContextFactory
    {
        readonly IHttpContextAccessor _httpContextAccessor;
        readonly FormOptions _formOptions;
        readonly IServiceScopeFactory _serviceScopeFactory;

        // This takes the IServiceProvider because it needs to support an ever expanding
        // set of services that flow down into HttpContext features
        public DefaultHttpContextFactory(IServiceProvider serviceProvider)
        {
            // May be null
            _httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            _formOptions = serviceProvider.GetRequiredService<IOptions<FormOptions>>().Value;
            _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        public HttpContext Create(IFeatureCollection featureCollection)
        {
            if (featureCollection == null)
                throw new ArgumentNullException(nameof(featureCollection));

            var httpContext = CreateHttpContext(featureCollection);
            if (_httpContextAccessor != null)
                _httpContextAccessor.HttpContext = httpContext;

            //httpContext.FormOptions = _formOptions;
            //httpContext.ServiceScopeFactory = _serviceScopeFactory;

            return httpContext;
        }

        static DefaultHttpContext CreateHttpContext(IFeatureCollection featureCollection) => featureCollection is IDefaultHttpContextContainer container
                ? container.HttpContext
                : new DefaultHttpContext(featureCollection);

        public void Dispose(HttpContext httpContext)
        {
            if (_httpContextAccessor != null)
                _httpContextAccessor.HttpContext = null;
        }
    }
}
