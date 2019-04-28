// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Contoso.GameNetCore.Proto.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Contoso.GameNetCore.Proto
{
    [Obsolete("This is obsolete and will be removed in a future version. Use DefaultProtoContextFactory instead.")]
    public class ProtoContextFactory : IProtoContextFactory
    {
        private readonly IProtoContextAccessor _httpContextAccessor;
        private readonly FormOptions _formOptions;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ProtoContextFactory(IOptions<FormOptions> formOptions)
            : this(formOptions, serviceScopeFactory: null)
        {
        }

        public ProtoContextFactory(IOptions<FormOptions> formOptions, IServiceScopeFactory serviceScopeFactory)
            : this(formOptions, serviceScopeFactory, httpContextAccessor: null)
        {
        }

        public ProtoContextFactory(IOptions<FormOptions> formOptions, IProtoContextAccessor httpContextAccessor)
            : this(formOptions, serviceScopeFactory: null, httpContextAccessor: httpContextAccessor)
        {
        }

        public ProtoContextFactory(IOptions<FormOptions> formOptions, IServiceScopeFactory serviceScopeFactory, IProtoContextAccessor httpContextAccessor)
        {
            if (formOptions == null)
            {
                throw new ArgumentNullException(nameof(formOptions));
            }

            if (serviceScopeFactory == null)
            {
                throw new ArgumentNullException(nameof(serviceScopeFactory));
            }

            _formOptions = formOptions.Value;
            _serviceScopeFactory = serviceScopeFactory;
            _httpContextAccessor = httpContextAccessor;
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
