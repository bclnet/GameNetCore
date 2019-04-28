// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Proto;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.Authentication
{
    /// <summary>
    /// Provides the appropriate IAuthenticationHandler instance for the authenticationScheme and request.
    /// </summary>
    public interface IAuthenticationHandlerProvider
    {
        /// <summary>
        /// Returns the handler instance that will be used.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="authenticationScheme">The name of the authentication scheme being handled.</param>
        /// <returns>The handler instance.</returns>
        Task<IAuthenticationHandler> GetHandlerAsync(ProtoContext context, string authenticationScheme);
    }
}