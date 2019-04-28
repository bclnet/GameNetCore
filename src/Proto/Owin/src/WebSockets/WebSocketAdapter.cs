// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.GameSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.Owin
{
    using GameSocketCloseAsync =
        Func<int /* closeStatus */,
            string /* closeDescription */,
            CancellationToken /* cancel */,
            Task>;
    using GameSocketReceiveAsync =
        Func<ArraySegment<byte> /* data */,
            CancellationToken /* cancel */,
            Task<Tuple<int /* messageType */,
                bool /* endOfMessage */,
                int /* count */>>>;
    using GameSocketReceiveTuple =
        Tuple<int /* messageType */,
            bool /* endOfMessage */,
            int /* count */>;
    using GameSocketSendAsync =
        Func<ArraySegment<byte> /* data */,
            int /* messageType */,
            bool /* endOfMessage */,
            CancellationToken /* cancel */,
            Task>;

    public class GameSocketAdapter
    {
        private readonly GameSocket _gameSocket;
        private readonly IDictionary<string, object> _environment;
        private readonly CancellationToken _cancellationToken;

        internal GameSocketAdapter(GameSocket gameSocket, CancellationToken ct)
        {
            _gameSocket = gameSocket;
            _cancellationToken = ct;

            _environment = new Dictionary<string, object>();
            _environment[OwinConstants.GameSocket.SendAsync] = new GameSocketSendAsync(SendAsync);
            _environment[OwinConstants.GameSocket.ReceiveAsync] = new GameSocketReceiveAsync(ReceiveAsync);
            _environment[OwinConstants.GameSocket.CloseAsync] = new GameSocketCloseAsync(CloseAsync);
            _environment[OwinConstants.GameSocket.CallCancelled] = ct;
            _environment[OwinConstants.GameSocket.Version] = OwinConstants.GameSocket.VersionValue;

            _environment[typeof(GameSocket).FullName] = gameSocket;
        }

        internal IDictionary<string, object> Environment
        {
            get { return _environment; }
        }

        internal Task SendAsync(ArraySegment<byte> buffer, int messageType, bool endOfMessage, CancellationToken cancel)
        {
            // Remap close messages to CloseAsync.  System.Net.GameSockets.GameSocket.SendAsync does not allow close messages.
            if (messageType == 0x8)
            {
                return RedirectSendToCloseAsync(buffer, cancel);
            }
            else if (messageType == 0x9 || messageType == 0xA)
            {
                // Ping & Pong, not allowed by the underlying APIs, silently discard.
                return Task.CompletedTask;
            }

            return _gameSocket.SendAsync(buffer, OpCodeToEnum(messageType), endOfMessage, cancel);
        }

        internal async Task<GameSocketReceiveTuple> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancel)
        {
            GameSocketReceiveResult nativeResult = await _gameSocket.ReceiveAsync(buffer, cancel);

            if (nativeResult.MessageType == GameSocketMessageType.Close)
            {
                _environment[OwinConstants.GameSocket.ClientCloseStatus] = (int)(nativeResult.CloseStatus ?? GameSocketCloseStatus.NormalClosure);
                _environment[OwinConstants.GameSocket.ClientCloseDescription] = nativeResult.CloseStatusDescription ?? string.Empty;
            }

            return new GameSocketReceiveTuple(
                EnumToOpCode(nativeResult.MessageType),
                nativeResult.EndOfMessage,
                nativeResult.Count);
        }

        internal Task CloseAsync(int status, string description, CancellationToken cancel)
        {
            return _gameSocket.CloseOutputAsync((GameSocketCloseStatus)status, description, cancel);
        }

        private Task RedirectSendToCloseAsync(ArraySegment<byte> buffer, CancellationToken cancel)
        {
            if (buffer.Array == null || buffer.Count == 0)
            {
                return CloseAsync(1000, string.Empty, cancel);
            }
            else if (buffer.Count >= 2)
            {
                // Unpack the close message.
                int statusCode =
                    (buffer.Array[buffer.Offset] << 8)
                        | buffer.Array[buffer.Offset + 1];
                string description = Encoding.UTF8.GetString(buffer.Array, buffer.Offset + 2, buffer.Count - 2);

                return CloseAsync(statusCode, description, cancel);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(buffer));
            }
        }

        internal async Task CleanupAsync()
        {
            switch (_gameSocket.State)
            {
                case GameSocketState.Closed: // Closed gracefully, no action needed. 
                case GameSocketState.Aborted: // Closed abortively, no action needed.                       
                    break;
                case GameSocketState.CloseReceived:
                    // Echo what the client said, if anything.
                    await _gameSocket.CloseAsync(_gameSocket.CloseStatus ?? GameSocketCloseStatus.NormalClosure,
                        _gameSocket.CloseStatusDescription ?? string.Empty, _cancellationToken);
                    break;
                case GameSocketState.Open:
                case GameSocketState.CloseSent: // No close received, abort so we don't have to drain the pipe.
                    _gameSocket.Abort();
                    break;
                default:
                    throw new NotSupportedException($"Unsupported {nameof(GameSocketState)} value: {_gameSocket.State}.");
            }
        }

        private static GameSocketMessageType OpCodeToEnum(int messageType)
        {
            switch (messageType)
            {
                case 0x1:
                    return GameSocketMessageType.Text;
                case 0x2:
                    return GameSocketMessageType.Binary;
                case 0x8:
                    return GameSocketMessageType.Close;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, string.Empty);
            }
        }

        private static int EnumToOpCode(GameSocketMessageType gameSocketMessageType)
        {
            switch (gameSocketMessageType)
            {
                case GameSocketMessageType.Text:
                    return 0x1;
                case GameSocketMessageType.Binary:
                    return 0x2;
                case GameSocketMessageType.Close:
                    return 0x8;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameSocketMessageType), gameSocketMessageType, string.Empty);
            }
        }
    }
}
