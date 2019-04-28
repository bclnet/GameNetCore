// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Http;
using Contoso.GameNetCore.Http.Features;
using Contoso.GameNetCore.GameSockets.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Contoso.GameNetCore.GameSockets
{
    public class GameSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly GameSocketOptions _options;
        private readonly ILogger _logger;
        private readonly bool _anyOriginAllowed;
        private readonly List<string> _allowedOrigins;

        public GameSocketMiddleware(RequestDelegate next, IOptions<GameSocketOptions> options, ILoggerFactory loggerFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
            _allowedOrigins = _options.AllowedOrigins.Select(o => o.ToLowerInvariant()).ToList();
            _anyOriginAllowed = _options.AllowedOrigins.Count == 0 || _options.AllowedOrigins.Contains("*", StringComparer.Ordinal);

            _logger = loggerFactory.CreateLogger<GameSocketMiddleware>();

            // TODO: validate options.
        }

        public Task Invoke(ProtoContext context)
        {
            // Detect if an opaque upgrade is available. If so, add a websocket upgrade.
            var upgradeFeature = context.Features.Get<IProtoUpgradeFeature>();
            if (upgradeFeature != null && context.Features.Get<IProtoGameSocketFeature>() == null)
            {
                var webSocketFeature = new UpgradeHandshake(context, upgradeFeature, _options);
                context.Features.Set<IProtoGameSocketFeature>(webSocketFeature);

                if (!_anyOriginAllowed)
                {
                    // Check for Origin header
                    var originHeader = context.Request.Headers[HeaderNames.Origin];

                    if (!StringValues.IsNullOrEmpty(originHeader) && webSocketFeature.IsGameSocketRequest)
                    {
                        // Check allowed origins to see if request is allowed
                        if (!_allowedOrigins.Contains(originHeader.ToString(), StringComparer.Ordinal))
                        {
                            _logger.LogDebug("Request origin {Origin} is not in the list of allowed origins.", originHeader);
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        }
                    }
                }
            }

            return _next(context);
        }

        private class UpgradeHandshake : IProtoGameSocketFeature
        {
            private readonly ProtoContext _context;
            private readonly IProtoUpgradeFeature _upgradeFeature;
            private readonly GameSocketOptions _options;
            private bool? _isGameSocketRequest;

            public UpgradeHandshake(ProtoContext context, IProtoUpgradeFeature upgradeFeature, GameSocketOptions options)
            {
                _context = context;
                _upgradeFeature = upgradeFeature;
                _options = options;
            }

            public bool IsGameSocketRequest
            {
                get
                {
                    if (_isGameSocketRequest == null)
                    {
                        if (!_upgradeFeature.IsUpgradableRequest)
                            _isGameSocketRequest = false;
                        else
                        {
                            var headers = new List<KeyValuePair<string, string>>();
                            foreach (var headerName in HandshakeHelpers.NeededHeaders)
                                foreach (var value in _context.Request.Headers.GetCommaSeparatedValues(headerName))
                                    headers.Add(new KeyValuePair<string, string>(headerName, value));
                            _isGameSocketRequest = HandshakeHelpers.CheckSupportedGameSocketRequest(_context.Request.Method, headers);
                        }
                    }
                    return _isGameSocketRequest.Value;
                }
            }

            public async Task<WebSocket> AcceptAsync(GameSocketAcceptContext acceptContext)
            {
                if (!IsGameSocketRequest)
                    throw new InvalidOperationException("Not a WebSocket request."); // TODO: LOC

                var subProtocol = acceptContext?.SubProtocol;
                var keepAliveInterval = _options.KeepAliveInterval;
                var receiveBufferSize = _options.ReceiveBufferSize;
                if (acceptContext is ExtendedGameSocketAcceptContext advancedAcceptContext)
                {
                    if (advancedAcceptContext.ReceiveBufferSize.HasValue)
                        receiveBufferSize = advancedAcceptContext.ReceiveBufferSize.Value;
                    if (advancedAcceptContext.KeepAliveInterval.HasValue)
                        keepAliveInterval = advancedAcceptContext.KeepAliveInterval.Value;
                }

                var key = string.Join(", ", _context.Request.Headers[Constants.Headers.SecGameSocketKey]);

                HandshakeHelpers.GenerateResponseHeaders(key, subProtocol, _context.Response.Headers);

                var opaqueTransport = await _upgradeFeature.UpgradeAsync(); // Sets status code to 101

                return WebSocket.CreateFromStream(opaqueTransport, isServer: true, subProtocol: subProtocol, keepAliveInterval: keepAliveInterval);
            }
        }
    }
}
