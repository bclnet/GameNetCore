// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETX
using Microsoft.AspNetCore.Http;
using ProtoContext = Microsoft.AspNetCore.Http.HttpContext;
#else
using Contoso.GameNetCore.Proto;
#endif
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Diagnostics;
#if NETX
using System.Net.Http.Headers;
#else
using System.Net.Proto.Headers;
#endif
using System.Runtime.CompilerServices;

namespace Contoso.GameNetCore.Hosting.Internal
{
    internal class HostingApplicationDiagnostics
    {
        static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        const string ActivityName = "Contoso.GameNetCore.Hosting.ProtoRequestIn";
        const string ActivityStartKey = "Contoso.GameNetCore.Hosting.ProtoRequestIn.Start";

        const string DeprecatedDiagnosticsBeginRequestKey = "Contoso.GameNetCore.Hosting.BeginRequest";
        const string DeprecatedDiagnosticsEndRequestKey = "Contoso.GameNetCore.Hosting.EndRequest";
        const string DiagnosticsUnhandledExceptionKey = "Contoso.GameNetCore.Hosting.UnhandledException";

        const string RequestIdHeaderName = "Request-Id";
        const string CorrelationContextHeaderName = "Correlation-Context";
        const string TraceParentHeaderName = "traceparent";
        const string TraceStateHeaderName = "tracestate";

        readonly DiagnosticListener _diagnosticListener;
        readonly ILogger _logger;

        public HostingApplicationDiagnostics(ILogger logger, DiagnosticListener diagnosticListener)
        {
            _logger = logger;
            _diagnosticListener = diagnosticListener;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BeginRequest(ProtoContext httpContext, ref HostingApplication.Context context)
        {
            var startTimestamp = 0L;

            if (HostingEventSource.Log.IsEnabled())
            {
                context.EventLogEnabled = true;
                // To keep the hot path short we defer logging in this function to non-inlines
                RecordRequestStartEventLog(httpContext);
            }

            var diagnosticListenerEnabled = _diagnosticListener.IsEnabled();
            var loggingEnabled = _logger.IsEnabled(LogLevel.Critical);

            if (diagnosticListenerEnabled)
            {
                if (_diagnosticListener.IsEnabled(ActivityName, httpContext))
                    context.Activity = StartActivity(httpContext);
                if (_diagnosticListener.IsEnabled(DeprecatedDiagnosticsBeginRequestKey))
                {
                    startTimestamp = Stopwatch.GetTimestamp();
                    RecordBeginRequestDiagnostics(httpContext, startTimestamp);
                }
            }

            // To avoid allocation, return a null scope if the logger is not on at least to some degree.
            if (loggingEnabled)
            {
                // Get the request ID (first try TraceParent header otherwise Request-ID header
                if (!httpContext.Request.Headers.TryGetValue(TraceParentHeaderName, out var correlationId))
                    httpContext.Request.Headers.TryGetValue(RequestIdHeaderName, out correlationId);

                // Scope may be relevant for a different level of logging, so we always create it
                // see: https://github.com/aspnet/Hosting/pull/944
                // Scope can be null if logging is not on.
                context.Scope = _logger.RequestScope(httpContext, correlationId);

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    if (startTimestamp == 0)
                        startTimestamp = Stopwatch.GetTimestamp();

                    // Non-inline
                    LogRequestStarting(httpContext);
                }
            }

            context.StartTimestamp = startTimestamp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RequestEnd(ProtoContext httpContext, Exception exception, HostingApplication.Context context)
        {
            // Local cache items resolved multiple items, in order of use so they are primed in cpu pipeline when used
            var startTimestamp = context.StartTimestamp;
            var currentTimestamp = 0L;

            // If startTimestamp was 0, then Information logging wasn't enabled at for this request (and calcuated time will be wildly wrong)
            // Is used as proxy to reduce calls to virtual: _logger.IsEnabled(LogLevel.Information)
            if (startTimestamp != 0)
            {
                currentTimestamp = Stopwatch.GetTimestamp();
                // Non-inline
                LogRequestFinished(httpContext, startTimestamp, currentTimestamp);
            }

            if (_diagnosticListener.IsEnabled())
            {
                if (currentTimestamp == 0)
                    currentTimestamp = Stopwatch.GetTimestamp();

                if (exception == null)
                {
                    // No exception was thrown, request was sucessful
                    if (_diagnosticListener.IsEnabled(DeprecatedDiagnosticsEndRequestKey))
                    {
                        // Diagnostics is enabled for EndRequest, but it may not be for BeginRequest
                        // so call GetTimestamp if currentTimestamp is zero (from above)
                        RecordEndRequestDiagnostics(httpContext, currentTimestamp);
                    }
                }
                else
                {
                    // Exception was thrown from request
                    if (_diagnosticListener.IsEnabled(DiagnosticsUnhandledExceptionKey))
                    {
                        // Diagnostics is enabled for UnhandledException, but it may not be for BeginRequest
                        // so call GetTimestamp if currentTimestamp is zero (from above)
                        RecordUnhandledExceptionDiagnostics(httpContext, currentTimestamp, exception);
                    }

                }

                var activity = context.Activity;
                // Always stop activity if it was started
                if (activity != null)
                    StopActivity(httpContext, activity);
            }

            if (context.EventLogEnabled && exception != null)
                // Non-inline
                HostingEventSource.Log.UnhandledException();

            // Logging Scope is finshed with
            context.Scope?.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ContextDisposed(HostingApplication.Context context)
        {
            if (context.EventLogEnabled)
                // Non-inline
                HostingEventSource.Log.RequestStop();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void LogRequestStarting(ProtoContext httpContext)
        {
            // IsEnabled is checked in the caller, so if we are here just log
            _logger.Log(
                logLevel: LogLevel.Information,
                eventId: LoggerEventIds.RequestStarting,
                state: new HostingRequestStartingLog(httpContext),
                exception: null,
                formatter: HostingRequestStartingLog.Callback);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void LogRequestFinished(ProtoContext httpContext, long startTimestamp, long currentTimestamp)
        {
            // IsEnabled isn't checked in the caller, startTimestamp > 0 is used as a fast proxy check
            // but that may be because diagnostics are enabled, which also uses startTimestamp, so check here
            if (_logger.IsEnabled(LogLevel.Information))
            {
                var elapsed = new TimeSpan((long)(TimestampToTicks * (currentTimestamp - startTimestamp)));

                _logger.Log(
                    logLevel: LogLevel.Information,
                    eventId: LoggerEventIds.RequestFinished,
                    state: new HostingRequestFinishedLog(httpContext, elapsed),
                    exception: null,
                    formatter: HostingRequestFinishedLog.Callback);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void RecordBeginRequestDiagnostics(ProtoContext httpContext, long startTimestamp)
        {
            _diagnosticListener.Write(
                DeprecatedDiagnosticsBeginRequestKey,
                new
                {
                    httpContext = httpContext,
                    timestamp = startTimestamp
                });
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void RecordEndRequestDiagnostics(ProtoContext httpContext, long currentTimestamp)
        {
            _diagnosticListener.Write(
                DeprecatedDiagnosticsEndRequestKey,
                new
                {
                    httpContext = httpContext,
                    timestamp = currentTimestamp
                });
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void RecordUnhandledExceptionDiagnostics(ProtoContext httpContext, long currentTimestamp, Exception exception)
        {
            _diagnosticListener.Write(
                DiagnosticsUnhandledExceptionKey,
                new
                {
                    httpContext = httpContext,
                    timestamp = currentTimestamp,
                    exception = exception
                });
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void RecordRequestStartEventLog(ProtoContext httpContext) =>
            HostingEventSource.Log.RequestStart(httpContext.Request.Method, httpContext.Request.Path);

        [MethodImpl(MethodImplOptions.NoInlining)]
        Activity StartActivity(ProtoContext httpContext)
        {
            var activity = new Activity(ActivityName);

            if (!httpContext.Request.Headers.TryGetValue(TraceParentHeaderName, out var requestId))
                httpContext.Request.Headers.TryGetValue(RequestIdHeaderName, out requestId);

            if (!StringValues.IsNullOrEmpty(requestId))
            {
                activity.SetParentId(requestId);
#if !NET2
                if (httpContext.Request.Headers.TryGetValue(TraceStateHeaderName, out var traceState))
                    activity.TraceStateString = traceState;
#endif

                // We expect baggage to be empty by default
                // Only very advanced users will be using it in near future, we encourage them to keep baggage small (few items)
                var baggage = httpContext.Request.Headers.GetCommaSeparatedValues(CorrelationContextHeaderName);
                if (baggage != StringValues.Empty)
                    foreach (var item in baggage)
                        if (NameValueHeaderValue.TryParse(item, out var baggageItem))
                            activity.AddBaggage(baggageItem.Name.ToString(), baggageItem.Value.ToString());
            }

            if (_diagnosticListener.IsEnabled(ActivityStartKey))
                _diagnosticListener.StartActivity(activity, new { ProtoContext = httpContext });
            else
                activity.Start();

            return activity;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void StopActivity(ProtoContext httpContext, Activity activity) =>
            _diagnosticListener.StopActivity(activity, new { ProtoContext = httpContext });
    }
}
