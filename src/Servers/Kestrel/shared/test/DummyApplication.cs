// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Contoso.GameNetCore.Hosting.Server;
using Contoso.GameNetCore.Http;
using Contoso.GameNetCore.Http.Features;

namespace Contoso.GameNetCore.Testing
{
    public class DummyApplication : IHttpApplication<HttpContext>
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly IHttpContextFactory _httpContextFactory;

        public DummyApplication()
            : this(_ => Task.CompletedTask)
        {
        }

        public DummyApplication(RequestDelegate requestDelegate)
            : this(requestDelegate, null)
        {
        }

        public DummyApplication(RequestDelegate requestDelegate, IHttpContextFactory httpContextFactory)
        {
            _requestDelegate = requestDelegate;
            _httpContextFactory = httpContextFactory;
        }

        public HttpContext CreateContext(IFeatureCollection contextFeatures)
        {
            return _httpContextFactory?.Create(contextFeatures) ?? new DefaultHttpContext(contextFeatures);
        }

        public void DisposeContext(HttpContext context, Exception exception)
        {
            _httpContextFactory?.Dispose(context);
        }

        public async Task ProcessRequestAsync(HttpContext context)
        {
            await _requestDelegate(context);
        }
    }
}
