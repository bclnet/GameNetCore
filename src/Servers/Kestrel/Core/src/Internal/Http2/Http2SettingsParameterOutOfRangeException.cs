// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    internal class Proto2SettingsParameterOutOfRangeException : Exception
    {
        public Proto2SettingsParameterOutOfRangeException(Proto2SettingsParameter parameter, long lowerBound, long upperBound)
            : base($"HTTP/2 SETTINGS parameter {parameter} must be set to a value between {lowerBound} and {upperBound}")
        {
            Parameter = parameter;
        }

        public Proto2SettingsParameter Parameter { get; }
    }
}
