// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Hosting.Server;
#if NETX
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using IProtoContextFactory = Microsoft.AspNetCore.Http.IHttpContextFactory;
using ProtoContext = Microsoft.AspNetCore.Http.HttpContext;
#else
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Proto.Features;
#endif
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.Hosting.Internal
{
    public class HostingApplication : IProtoApplication<HostingApplication.Context>
    {
        readonly RequestDelegate _application;
        readonly IProtoContextFactory _protoContextFactory;
        HostingApplicationDiagnostics _diagnostics;

        public HostingApplication(
            RequestDelegate application,
            ILogger logger,
            DiagnosticListener diagnosticSource,
            IProtoContextFactory protoContextFactory)
        {
            _application = application;
            _diagnostics = new HostingApplicationDiagnostics(logger, diagnosticSource);
            _protoContextFactory = protoContextFactory;
        }

        // Set up the request
        public Context CreateContext(IFeatureCollection contextFeatures)
        {
            var context = new Context();
            var protoContext = _protoContextFactory.Create(contextFeatures);

            _diagnostics.BeginRequest(protoContext, ref context);

            context.ProtoContext = protoContext;
            return context;
        }

        // Execute the request
        public Task ProcessRequestAsync(Context context) =>
            _application(context.ProtoContext);

        // Clean up the request
        public void DisposeContext(Context context, Exception exception)
        {
            var protoContext = context.ProtoContext;
            _diagnostics.RequestEnd(protoContext, exception, context);
            _protoContextFactory.Dispose(protoContext);
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
