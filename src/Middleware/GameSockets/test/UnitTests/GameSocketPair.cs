using System;
using System.Net.GameSockets;
using Contoso.GameNetCore.GameSockets.Internal;

namespace Contoso.GameNetCore.GameSockets.Test
{
    internal class GameSocketPair
    {
        public GameSocket ClientSocket { get; }
        public GameSocket ServerSocket { get; }
        public DuplexStream ServerStream { get; }
        public DuplexStream ClientStream { get; }

        public GameSocketPair(DuplexStream serverStream, DuplexStream clientStream, GameSocket clientSocket, GameSocket serverSocket)
        {
            ClientStream = clientStream;
            ServerStream = serverStream;
            ClientSocket = clientSocket;
            ServerSocket = serverSocket;
        }

        public static GameSocketPair Create()
        {
            // Create streams
            var serverStream = new DuplexStream();
            var clientStream = serverStream.CreateReverseDuplexStream();

            return new GameSocketPair(
                serverStream,
                clientStream,
                clientSocket: GameSocket.CreateFromStream(clientStream, isServer: false, subProtocol: null, keepAliveInterval: TimeSpan.FromMinutes(2)),
                serverSocket: GameSocket.CreateFromStream(serverStream, isServer: true, subProtocol: null, keepAliveInterval: TimeSpan.FromMinutes(2)));
        }
    }
}
