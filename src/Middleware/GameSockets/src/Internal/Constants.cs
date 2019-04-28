// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.GameSockets.Internal
{
    public static class Constants
    {
        public static class Headers
        {
            public const string Upgrade = "Upgrade";
            public const string UpgradeGameSocket = "gamesocket";
            public const string Connection = "Connection";
            public const string ConnectionUpgrade = "Upgrade";
            public const string SecGameSocketKey = "Sec-GameSocket-Key";
            public const string SecGameSocketVersion = "Sec-GameSocket-Version";
            public const string SecGameSocketProtocol = "Sec-GameSocket-Protocol";
            public const string SecGameSocketAccept = "Sec-GameSocket-Accept";
            public const string SupportedVersion = "13";
        }
    }
}
