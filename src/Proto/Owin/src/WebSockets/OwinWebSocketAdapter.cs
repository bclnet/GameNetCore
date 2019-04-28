// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.GameSockets;

namespace Contoso.GameNetCore.Owin
{
    // http://owin.org/extensions/owin-GameSocket-Extension-v0.4.0.htm
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
    using GameSocketSendAsync =
        Func<ArraySegment<byte> /* data */,
            int /* messageType */,
            bool /* endOfMessage */,
            CancellationToken /* cancel */,
            Task>;
    using RawGameSocketReceiveResult = Tuple<int, // type
        bool, // end of message?
        int>; // count

    public class OwinGameSocketAdapter : GameSocket
    {
        private const int _rentedBufferSize = 1024;
        private IDictionary<string, object> _gamesocketContext;
        private GameSocketSendAsync _sendAsync;
        private GameSocketReceiveAsync _receiveAsync;
        private GameSocketCloseAsync _closeAsync;
        private GameSocketState _state;
        private string _subProtocol;

        public OwinGameSocketAdapter(IDictionary<string, object> gamesocketContext, string subProtocol)
        {
            _gamesocketContext = gamesocketContext;
            _sendAsync = (GameSocketSendAsync)gamesocketContext[OwinConstants.GameSocket.SendAsync];
            _receiveAsync = (GameSocketReceiveAsync)gamesocketContext[OwinConstants.GameSocket.ReceiveAsync];
            _closeAsync = (GameSocketCloseAsync)gamesocketContext[OwinConstants.GameSocket.CloseAsync];
            _state = GameSocketState.Open;
            _subProtocol = subProtocol;
        }

        public override GameSocketCloseStatus? CloseStatus
        {
            get
            {
                object obj;
                if (_gamesocketContext.TryGetValue(OwinConstants.GameSocket.ClientCloseStatus, out obj))
                {
                    return (GameSocketCloseStatus)obj;
                }
                return null;
            }
        }

        public override string CloseStatusDescription
        {
            get
            {
                object obj;
                if (_gamesocketContext.TryGetValue(OwinConstants.GameSocket.ClientCloseDescription, out obj))
                {
                    return (string)obj;
                }
                return null;
            }
        }

        public override string SubProtocol
        {
            get
            {
                return _subProtocol;
            }
        }

        public override GameSocketState State
        {
            get
            {
                return _state;
            }
        }

        public override async Task<GameSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            var rawResult = await _receiveAsync(buffer, cancellationToken);
            var messageType = OpCodeToEnum(rawResult.Item1);
            if (messageType == GameSocketMessageType.Close)
            {
                if (State == GameSocketState.Open)
                {
                    _state = GameSocketState.CloseReceived;
                }
                else if (State == GameSocketState.CloseSent)
                {
                    _state = GameSocketState.Closed;
                }
                return new GameSocketReceiveResult(rawResult.Item3, messageType, rawResult.Item2, CloseStatus, CloseStatusDescription);
            }
            else
            {
                return new GameSocketReceiveResult(rawResult.Item3, messageType, rawResult.Item2);
            }
        }

        public override Task SendAsync(ArraySegment<byte> buffer, GameSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            return _sendAsync(buffer, EnumToOpCode(messageType), endOfMessage, cancellationToken);
        }

        public override async Task CloseAsync(GameSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            if (State == GameSocketState.Open || State == GameSocketState.CloseReceived)
            {
                await CloseOutputAsync(closeStatus, statusDescription, cancellationToken);
            }

            var buffer = ArrayPool<byte>.Shared.Rent(_rentedBufferSize);
            try
            {
                while (State == GameSocketState.CloseSent)
                {
                    // Drain until close received
                    await ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public override Task CloseOutputAsync(GameSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            // TODO: Validate state
            if (State == GameSocketState.Open)
            {
                _state = GameSocketState.CloseSent;
            }
            else if (State == GameSocketState.CloseReceived)
            {
                _state = GameSocketState.Closed;
            }
            return _closeAsync((int)closeStatus, statusDescription, cancellationToken);
        }

        public override void Abort()
        {
            _state = GameSocketState.Aborted;
        }

        public override void Dispose()
        {
            _state = GameSocketState.Closed;
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