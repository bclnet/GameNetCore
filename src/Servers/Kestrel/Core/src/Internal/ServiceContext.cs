// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO.Pipelines;
using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto;
using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Infrastructure;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal
{
    internal class ServiceContext
    {
        public IKestrelTrace Log { get; set; }

        public PipeScheduler Scheduler { get; set; }

        public IProtoParser<Proto1ParsingHandler> ProtoParser { get; set; }

        public ISystemClock SystemClock { get; set; }

        public DateHeaderValueManager DateHeaderValueManager { get; set; }

        public ConnectionManager ConnectionManager { get; set; }

        public Heartbeat Heartbeat { get; set; }

        public KestrelServerOptions ServerOptions { get; set; }
    }
}
