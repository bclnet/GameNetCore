// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Hosting.Server;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Proto.Features;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.Hosting.Internal
{
    public class HostingApplication : IProtoApplication<HostingApplication.Context>
    {
        readonly RequestDelegate _application;
        readonly IProtoContextFactory _httpContextFactory;
        HostingApplicationDiagnostics _diagnostics;

        public HostingApplication(
            RequestDelegate application,
            ILogger logger,
            DiagnosticListener diagnosticSource,
            IProtoContextFactory httpContextFactory)
        {
            _application = application;
            _diagnostics = new HostingApplicationDiagnostics(logger, diagnosticSource);
            _httpContextFactory = httpContextFactory;
        }

        // Set up the request
        public Context CreateContext(IFeatureCollection contextFeatures)
        {
            var context = new Context();
            var httpContext = _httpContextFactory.Create(contextFeatures);

            _diagnostics.BeginRequest(httpContext, ref context);

            context.ProtoContext = httpContext;
            return context;
        }

        // Execute the request
        public Task ProcessRequestAsync(Context context) =>
            _application(context.ProtoContext);

        // Clean up the request
        public void DisposeContext(Context context, Exception exception)
        {
            var httpContext = context.ProtoContext;
            _diagnostics.RequestEnd(httpContext, exception, context);
            _httpContextFactory.Dispose(httpContext);
            _diagnostics.ContextDisposed(context);
        }

        public struct Context
        {
            public ProtoContext ProtoContext { get; set; }
            public IDisposable Scope { get; set; }
            public long StartTimestamp { get; set; }
            public bool EventLogEnabled { get; set; }
            public Activity Activity { get; set; }
        }
    }
}
