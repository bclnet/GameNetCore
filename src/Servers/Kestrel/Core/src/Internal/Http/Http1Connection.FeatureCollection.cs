// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Proto.Features;
using Contoso.GameNetCore.Server.Kestrel.Core.Features;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto
{
    internal partial class Proto1Connection : IProtoMinRequestBodyDataRateFeature,
                                           IProtoMinResponseDataRateFeature
    {
        MinDataRate IProtoMinRequestBodyDataRateFeature.MinDataRate
        {
            get => MinRequestBodyDataRate;
            set => MinRequestBodyDataRate = value;
        }

        MinDataRate IProtoMinResponseDataRateFeature.MinDataRate
        {
            get => MinResponseDataRate;
            set => MinResponseDataRate = value;
        }
    }
}
