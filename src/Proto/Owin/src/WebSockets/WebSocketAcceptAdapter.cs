// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.GameSockets;
using System.Threading;
using System.Threading.Tasks;
using Contoso.GameNetCore.Proto;

namespace Contoso.GameNetCore.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using GameSocketAccept =
        Action
        <
            IDictionary<string, object>, // GameSocket Accept parameters
            Func // GameSocketFunc callback
            <
                IDictionary<string, object>, // GameSocket environment
                Task // Complete
            >
        >;
    using GameSocketAcceptAlt =
        Func
        <
            GameSocketAcceptContext, // GameSocket Accept parameters
            Task<GameSocket>
        >;

    /// <summary>
    /// This adapts the ASP.NET Core GameSocket Accept flow to match the OWIN GameSocket accept flow.
    /// This enables OWIN based components to use GameSockets on ASP.NET Core servers.
    /// </summary>
    public class GameSocketAcceptAdapter
    {
        private IDictionary<string, object> _env;
        private GameSocketAcceptAlt _accept;
        private AppFunc _callback;
        private IDictionary<string, object> _options;

        public GameSocketAcceptAdapter(IDictionary<string, object> env, GameSocketAcceptAlt accept)
	    {
            _env = env;
            _accept = accept;
        }

        private void AcceptGameSocket(IDictionary<string, object> options, AppFunc callback)
        {
            _options = options;
            _callback = callback;
            _env[OwinConstants.ResponseStatusCode] = 101;
        }

        public static AppFunc AdaptGameSockets(AppFunc next)
        {
            return async environment =>
            {
                object accept;
                if (environment.TryGetValue(OwinConstants.GameSocket.AcceptAlt, out accept) && accept is GameSocketAcceptAlt)
                {
                    var adapter = new GameSocketAcceptAdapter(environment, (GameSocketAcceptAlt)accept);

                    environment[OwinConstants.GameSocket.Accept] = new GameSocketAccept(adapter.AcceptGameSocket);
                    await next(environment);
                    if ((int)environment[OwinConstants.ResponseStatusCode] == 101 && adapter._callback != null)
                    {
                        GameSocketAcceptContext acceptContext = null;
                        object obj;
                        if (adapter._options != null && adapter._options.TryGetValue(typeof(GameSocketAcceptContext).FullName, out obj))
                        {
                            acceptContext = obj as GameSocketAcceptContext;
                        }
                        else if (adapter._options != null)
                        {
                            acceptContext = new OwinGameSocketAcceptContext(adapter._options);
                        }

                        var gameSocket = await adapter._accept(acceptContext);
                        var gameSocketAdapter = new GameSocketAdapter(gameSocket, (CancellationToken)environment[OwinConstants.CallCancelled]);
                        await adapter._callback(gameSocketAdapter.Environment);
                        await gameSocketAdapter.CleanupAsync();
                    }
                }
                else
                {
                    await next(environment);
                }
            };
        }
    }
}