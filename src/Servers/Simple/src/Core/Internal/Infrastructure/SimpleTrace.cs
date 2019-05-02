// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using Microsoft.Extensions.Logging;
using System;

namespace Contoso.GameNetCore.Server.Simple.Core.Internal.Infrastructure
{
    internal class SimpleTrace : ISimpleTrace
    {
        protected readonly ILogger _logger;

        public SimpleTrace(ILogger logger)
        {
            _logger = logger;
        }

        public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => _logger.Log(logLevel, eventId, state, exception, formatter);

        public virtual bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public virtual IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);
    }
}
