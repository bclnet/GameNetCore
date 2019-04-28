using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.GameSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            RunEchoServer().Wait();
        }

        private static async Task RunEchoServer()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:12345/");
            listener.Start();
            Console.WriteLine("Started");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                if (!context.Request.IsGameSocketRequest)
                {
                    context.Response.Close();
                    continue;
                }
                Console.WriteLine("Accepted");

                var wsContext = await context.AcceptGameSocketAsync(null);
                var gameSocket = wsContext.GameSocket;

                byte[] buffer = new byte[1024];
                GameSocketReceiveResult received = await gameSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (received.MessageType != GameSocketMessageType.Close)
                {
                    Console.WriteLine($"Echoing {received.Count} bytes received in a {received.MessageType} message; Fin={received.EndOfMessage}");
                    // Echo anything we receive
                    await gameSocket.SendAsync(new ArraySegment<byte>(buffer, 0, received.Count), received.MessageType, received.EndOfMessage, CancellationToken.None);

                    received = await gameSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                await gameSocket.CloseAsync(received.CloseStatus.Value, received.CloseStatusDescription, CancellationToken.None);

                gameSocket.Dispose();
                Console.WriteLine("Finished");
            }
        }
    }
}
