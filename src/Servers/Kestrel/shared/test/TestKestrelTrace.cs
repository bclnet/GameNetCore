// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Infrastructure;

namespace Contoso.GameNetCore.Testing
{
    internal class TestKestrelTrace : KestrelTrace
    {
        public TestKestrelTrace() : this(new TestApplicationErrorLogger())
        {
        }

        public TestKestrelTrace(TestApplicationErrorLogger testLogger) : base(testLogger)
        {
            Logger = testLogger;
        }

        public TestApplicationErrorLogger Logger { get; private set; }
    }
}
