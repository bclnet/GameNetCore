// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    internal class Proto2ConnectionErrorException : Exception
    {
        public Proto2ConnectionErrorException(string message, Proto2ErrorCode errorCode)
            : base($"HTTP/2 connection error ({errorCode}): {message}")
        {
            ErrorCode = errorCode;
        }

        public Proto2ErrorCode ErrorCode { get; }
    }
}
