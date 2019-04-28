// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2.FlowControl;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Infrastructure
{
    internal interface ITimeoutControl
    {
        TimeoutReason TimerReason { get; }

        void SetTimeout(long ticks, TimeoutReason timeoutReason);
        void ResetTimeout(long ticks, TimeoutReason timeoutReason);
        void CancelTimeout();

        void InitializeProto2(InputFlowControl connectionInputFlowControl);
        void StartRequestBody(MinDataRate minRate);
        void StopRequestBody();
        void StartTimingRead();
        void StopTimingRead();
        void BytesRead(long count);

        void StartTimingWrite();
        void StopTimingWrite();
        void BytesWrittenToBuffer(MinDataRate minRate, long count);
    }
}
