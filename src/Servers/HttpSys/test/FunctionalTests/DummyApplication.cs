// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Contoso.GameNetCore.Hosting.Server;
using Contoso.GameNetCore.Http;
using Contoso.GameNetCore.Http.Features;

namespace Contoso.GameNetCore.Server.HttpSys
{
    internal class DummyApplication : IHttpApplication<HttpContext>
    {
        private readonly RequestDelegate _requestDelegate;

        public DummyApplication() : this(context => Task.CompletedTask) { }

        public DummyApplication(RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public HttpContext CreateContext(IFeatureCollection contextFeatures)
        {
            return new DefaultHttpContext(contextFeatures);
        }

        public void DisposeContext(HttpContext httpContext, Exception exception)
        {

        }

        public async Task ProcessRequestAsync(HttpContext httpContext)
        {
            await _requestDelegate(httpContext);
        }
    }
}
