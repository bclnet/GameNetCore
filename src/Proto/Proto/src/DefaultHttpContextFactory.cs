// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Proto.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Contoso.GameNetCore.Proto
{
    public class DefaultProtoContextFactory : IProtoContextFactory
    {
        private readonly IProtoContextAccessor _httpContextAccessor;
        private readonly FormOptions _formOptions;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        // This takes the IServiceProvider because it needs to support an ever expanding
        // set of services that flow down into ProtoContext features
        public DefaultProtoContextFactory(IServiceProvider serviceProvider)
        {
            // May be null
            _httpContextAccessor = serviceProvider.GetService<IProtoContextAccessor>();
            _formOptions = serviceProvider.GetRequiredService<IOptions<FormOptions>>().Value;
            _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        public ProtoContext Create(IFeatureCollection featureCollection)
        {
            if (featureCollection == null)
            {
                throw new ArgumentNullException(nameof(featureCollection));
            }

            var httpContext = CreateProtoContext(featureCollection);
            if (_httpContextAccessor != null)
            {
                _httpContextAccessor.ProtoContext = httpContext;
            }

            httpContext.FormOptions = _formOptions;
            httpContext.ServiceScopeFactory = _serviceScopeFactory;

            return httpContext;
        }

        private static DefaultProtoContext CreateProtoContext(IFeatureCollection featureCollection)
        {
            if (featureCollection is IDefaultProtoContextContainer container)
            {
                return container.ProtoContext;
            }

            return new DefaultProtoContext(featureCollection);
        }

        public void Dispose(ProtoContext httpContext)
        {
            if (_httpContextAccessor != null)
            {
                _httpContextAccessor.ProtoContext = null;
            }
        }
    }
}
