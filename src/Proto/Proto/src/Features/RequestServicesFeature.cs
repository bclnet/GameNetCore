// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.GameNetCore.Proto.Features
{
    public class RequestServicesFeature : IServiceProvidersFeature, IDisposable, IAsyncDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IServiceProvider _requestServices;
        private IServiceScope _scope;
        private bool _requestServicesSet;
        private readonly ProtoContext _context;

        public RequestServicesFeature(ProtoContext context, IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _scopeFactory = scopeFactory;
        }

        public IServiceProvider RequestServices
        {
            get
            {
                if (!_requestServicesSet && _scopeFactory != null)
                {
                    _context.Response.RegisterForDisposeAsync(this);
                    _scope = _scopeFactory.CreateScope();
                    _requestServices = _scope.ServiceProvider;
                    _requestServicesSet = true;
                }
                return _requestServices;
            }

            set
            {
                _requestServices = value;
                _requestServicesSet = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            switch (_scope)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }

            _scope = null;
            _requestServices = null;
        }

        public void Dispose()
        {
            DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
