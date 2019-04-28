// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.GameSockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Contoso.GameNetCore.Hosting.Server;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Proto.Features;
using Context = Microsoft.GameNetCore.Hosting.Internal.HostingApplication.Context;

namespace Contoso.GameNetCore.TestHost
{
    public class GameSocketClient
    {
        private readonly IProtoApplication<Context> _application;
        private readonly PathString _pathBase;

        internal GameSocketClient(PathString pathBase, IProtoApplication<Context> application)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));

            // PathString.StartsWithSegments that we use below requires the base path to not end in a slash.
            if (pathBase.HasValue && pathBase.Value.EndsWith("/"))
            {
                pathBase = new PathString(pathBase.Value.Substring(0, pathBase.Value.Length - 1));
            }
            _pathBase = pathBase;

            SubProtocols = new List<string>();
        }

        public IList<string> SubProtocols
        {
            get;
            private set;
        }

        public Action<ProtoRequest> ConfigureRequest
        {
            get;
            set;
        }

        internal bool AllowSynchronousIO { get; set; }

        public async Task<GameSocket> ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            GameSocketFeature gameSocketFeature = null;
            var contextBuilder = new ProtoContextBuilder(_application, AllowSynchronousIO);
            contextBuilder.Configure(context =>
            {
                var request = context.Request;
                var scheme = uri.Scheme;
                scheme = (scheme == "ws") ? "http" : scheme;
                scheme = (scheme == "wss") ? "https" : scheme;
                request.Scheme = scheme;
                request.Path = PathString.FromUriComponent(uri);
                request.PathBase = PathString.Empty;
                if (request.Path.StartsWithSegments(_pathBase, out var remainder))
                {
                    request.Path = remainder;
                    request.PathBase = _pathBase;
                }
                request.QueryString = QueryString.FromUriComponent(uri);
                request.Headers.Add("Connection", new string[] { "Upgrade" });
                request.Headers.Add("Upgrade", new string[] { "gamesocket" });
                request.Headers.Add("Sec-GameSocket-Version", new string[] { "13" });
                request.Headers.Add("Sec-GameSocket-Key", new string[] { CreateRequestKey() });
                request.Body = Stream.Null;

                // GameSocket
                gameSocketFeature = new GameSocketFeature(context);
                context.Features.Set<IProtoGameSocketFeature>(gameSocketFeature);

                ConfigureRequest?.Invoke(context.Request);
            });

            var httpContext = await contextBuilder.SendAsync(cancellationToken);

            if (httpContext.Response.StatusCode != StatusCodes.Status101SwitchingProtocols)
            {
                throw new InvalidOperationException("Incomplete handshake, status code: " + httpContext.Response.StatusCode);
            }
            if (gameSocketFeature.ClientGameSocket == null)
            {
                throw new InvalidOperationException("Incomplete handshake");
            }

            return gameSocketFeature.ClientGameSocket;
        }

        private string CreateRequestKey()
        {
            byte[] data = new byte[16];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(data);
            return Convert.ToBase64String(data);
        }

        private class GameSocketFeature : IProtoGameSocketFeature
        {
            private readonly ProtoContext _httpContext;

            public GameSocketFeature(ProtoContext context)
            {
                _httpContext = context;
            }

            bool IProtoGameSocketFeature.IsGameSocketRequest => true;

            public GameSocket ClientGameSocket { get; private set; }

            public GameSocket ServerGameSocket { get; private set; }

            async Task<GameSocket> IProtoGameSocketFeature.AcceptAsync(GameSocketAcceptContext context)
            {
                var gamesockets = TestGameSocket.CreatePair(context.SubProtocol);
                if (_httpContext.Response.HasStarted)
                {
                    throw new InvalidOperationException("The response has already started");
                }

                _httpContext.Response.StatusCode = StatusCodes.Status101SwitchingProtocols;
                ClientGameSocket = gamesockets.Item1;
                ServerGameSocket = gamesockets.Item2;
                await _httpContext.Response.Body.FlushAsync(_httpContext.RequestAborted); // Send headers to the client
                return ServerGameSocket;
            }
        }
    }
}
