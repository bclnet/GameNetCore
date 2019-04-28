// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.GameSockets;
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
    /// This adapts the OWIN GameSocket accept flow to match the ASP.NET Core GameSocket Accept flow.
    /// This enables ASP.NET Core components to use GameSockets on OWIN based servers.
    /// </summary>
    public class OwinGameSocketAcceptAdapter
    {
        private GameSocketAccept _owinGameSocketAccept;
        private TaskCompletionSource<int> _requestTcs = new TaskCompletionSource<int>();
        private TaskCompletionSource<GameSocket> _acceptTcs = new TaskCompletionSource<GameSocket>();
        private TaskCompletionSource<int> _upstreamWentAsync = new TaskCompletionSource<int>();
        private string _subProtocol = null;

        private OwinGameSocketAcceptAdapter(GameSocketAccept owinGameSocketAccept)
        {
            _owinGameSocketAccept = owinGameSocketAccept;
        }

        private Task RequestTask { get { return _requestTcs.Task; } }
        private Task UpstreamTask { get; set; }
        private TaskCompletionSource<int> UpstreamWentAsyncTcs { get { return _upstreamWentAsync; } }

        private async Task<GameSocket> AcceptGameSocketAsync(GameSocketAcceptContext context)
        {
            IDictionary<string, object> options = null;
            if (context is OwinGameSocketAcceptContext)
            {
                var acceptContext = context as OwinGameSocketAcceptContext;
                options = acceptContext.Options;
                _subProtocol = acceptContext.SubProtocol;
            }
            else if (context?.SubProtocol != null)
            {
                options = new Dictionary<string, object>(1)
                {
                    { OwinConstants.GameSocket.SubProtocol, context.SubProtocol }
                };
                _subProtocol = context.SubProtocol;
            }

            // Accept may have been called synchronously on the original request thread, we might not have a task yet. Go async.
            await _upstreamWentAsync.Task;

            _owinGameSocketAccept(options, OwinAcceptCallback);
            _requestTcs.TrySetResult(0); // Let the pipeline unwind.

            return await _acceptTcs.Task;
        }

        private Task OwinAcceptCallback(IDictionary<string, object> gameSocketContext)
        {
            _acceptTcs.TrySetResult(new OwinGameSocketAdapter(gameSocketContext, _subProtocol));
            return UpstreamTask;
        }

        // Make sure declined gamesocket requests complete. This is a no-op for accepted gamesocket requests.
        private void EnsureCompleted(Task task)
        {
            if (task.IsCanceled)
            {
                _requestTcs.TrySetCanceled();
            }
            else if (task.IsFaulted)
            {
                _requestTcs.TrySetException(task.Exception);
            }
            else
            {
                _requestTcs.TrySetResult(0);
            }
        }

        // Order of operations:
        // 1. A GameSocket handshake request is received by the middleware.
        // 2. The middleware inserts an alternate Accept signature into the OWIN environment.
        // 3. The middleware invokes Next and stores Next's Task locally. It then returns an alternate Task to the server.
        // 4. The OwinFeatureCollection adapts the alternate Accept signature to IProtoGameSocketFeature.AcceptAsync.
        // 5. A component later in the pipeline invokes IProtoGameSocketFeature.AcceptAsync (mapped to AcceptGameSocketAsync).
        // 6. The middleware calls the OWIN Accept, providing a local callback, and returns an incomplete Task.
        // 7. The middleware completes the alternate Task it returned from Invoke, telling the server that the request pipeline has completed.
        // 8. The server invokes the middleware's callback, which creates a GameSocket adapter and completes the original Accept Task with it.
        // 9. The middleware waits while the application uses the GameSocket, where the end is signaled by the Next's Task completion.
        public static AppFunc AdaptGameSockets(AppFunc next)
        {
            return environment =>
            {
                object accept;
                if (environment.TryGetValue(OwinConstants.GameSocket.Accept, out accept) && accept is GameSocketAccept)
                {
                    var adapter = new OwinGameSocketAcceptAdapter((GameSocketAccept)accept);

                    environment[OwinConstants.GameSocket.AcceptAlt] = new GameSocketAcceptAlt(adapter.AcceptGameSocketAsync);

                    try
                    {
                        adapter.UpstreamTask = next(environment);
                        adapter.UpstreamWentAsyncTcs.TrySetResult(0);
                        adapter.UpstreamTask.ContinueWith(adapter.EnsureCompleted, TaskContinuationOptions.ExecuteSynchronously);
                    }
                    catch (Exception ex)
                    {
                        adapter.UpstreamWentAsyncTcs.TrySetException(ex);
                        throw;
                    }

                    return adapter.RequestTask;
                }
                else
                {
                    return next(environment);
                }
            };
        }
    }
}