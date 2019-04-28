// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETX
using Microsoft.AspNetCore.Http;
using ProtoContext = Microsoft.AspNetCore.Http.HttpContext;
#else
using Contoso.GameNetCore.Proto;
#endif
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Contoso.GameNetCore.Hosting.Internal
{
    internal static class HostingLoggerExtensions
    {
        public static IDisposable RequestScope(this ILogger logger, ProtoContext httpContext, string correlationId) =>
            logger.BeginScope(new HostingLogScope(httpContext, correlationId));

        public static void ApplicationError(this ILogger logger, Exception exception) =>
            logger.ApplicationError(
                eventId: LoggerEventIds.ApplicationStartupException,
                message: "Application startup exception",
                exception: exception);

        public static void HostingStartupAssemblyError(this ILogger logger, Exception exception) =>
            logger.ApplicationError(
                eventId: LoggerEventIds.HostingStartupAssemblyException,
                message: "Hosting startup assembly exception",
                exception: exception);

        public static void ApplicationError(this ILogger logger, EventId eventId, string message, Exception exception)
        {
            if (exception is ReflectionTypeLoadException reflectionTypeLoadException)
                foreach (var ex in reflectionTypeLoadException.LoaderExceptions)
                    message = message + Environment.NewLine + ex.Message;

            logger.LogCritical(
                eventId: eventId,
                message: message,
                exception: exception);
        }

        public static void Starting(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug(
                   eventId: LoggerEventIds.Starting,
                   message: "Hosting starting");
        }

        public static void Started(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug(
                    eventId: LoggerEventIds.Started,
                    message: "Hosting started");
        }

        public static void Shutdown(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug(
                    eventId: LoggerEventIds.Shutdown,
                    message: "Hosting shutdown");
        }

        public static void ServerShutdownException(this ILogger logger, Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug(
                    eventId: LoggerEventIds.ServerShutdownException,
                    exception: ex,
                    message: "Server shutdown exception");
        }

        class HostingLogScope : IReadOnlyList<KeyValuePair<string, object>>
        {
            readonly string _path;
            readonly string _traceIdentifier;
            readonly string _correlationId;

            string _cachedToString;

            public int Count => 3;

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    if (index == 0) return new KeyValuePair<string, object>("RequestId", _traceIdentifier);
                    else if (index == 1) return new KeyValuePair<string, object>("RequestPath", _path);
                    else if (index == 2) return new KeyValuePair<string, object>("CorrelationId", _correlationId);
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }

            public HostingLogScope(ProtoContext httpContext, string correlationId)
            {
                _traceIdentifier = httpContext.TraceIdentifier;
                _path = (httpContext.Request.PathBase.HasValue
                         ? httpContext.Request.PathBase + httpContext.Request.Path
                         : httpContext.Request.Path).ToString();
                _correlationId = correlationId;
            }

            public override string ToString()
            {
                if (_cachedToString == null)
                    _cachedToString = string.Format(
                        CultureInfo.InvariantCulture,
                        "RequestId:{0} RequestPath:{1}",
                        _traceIdentifier,
                        _path);
                return _cachedToString;
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (var i = 0; i < Count; ++i)
                    yield return this[i];
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}

