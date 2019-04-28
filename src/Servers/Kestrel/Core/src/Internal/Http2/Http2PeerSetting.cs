// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    internal readonly struct Proto2PeerSetting
    {
        public Proto2PeerSetting(Proto2SettingsParameter parameter, uint value)
        {
            Parameter = parameter;
            Value = value;
        }

        public Proto2SettingsParameter Parameter { get; }

        public uint Value { get; }
    }
}
