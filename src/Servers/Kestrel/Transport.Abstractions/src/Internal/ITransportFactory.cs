// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Server.Kestrel.Transport.Abstractions.Internal
{
    public interface ITransportFactory
    {
        ITransport Create(IEndPointInformation endPointInformation, IConnectionDispatcher dispatcher);
    }
}
