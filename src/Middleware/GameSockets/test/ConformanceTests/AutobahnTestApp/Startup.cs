using System;
using System.Net.GameSockets;
using System.Threading;
using System.Threading.Tasks;
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Hosting;
using Contoso.GameNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AutobahnTestApp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseGameSockets();

            var logger = loggerFactory.CreateLogger<Startup>();
            app.Use(async (context, next) =>
            {
                if (context.GameSockets.IsGameSocketRequest)
                {
                    logger.LogInformation("Received GameSocket request");
                    using (var gameSocket = await context.GameSockets.AcceptGameSocketAsync())
                    {
                        await Echo(gameSocket, context.RequestAborted);
                    }
                }
                else
                {
                    var wsScheme = context.Request.IsHttps ? "wss" : "ws";
                    var wsUrl = $"{wsScheme}://{context.Request.Host.Host}:{context.Request.Host.Port}{context.Request.Path}";
                    await context.Response.WriteAsync($"Ready to accept a GameSocket request at: {wsUrl}");
                }
            });

        }

        private async Task Echo(GameSocket gameSocket, CancellationToken cancellationToken)
        {
            var buffer = new byte[1024 * 4];
            var result = await gameSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            while (!result.CloseStatus.HasValue)
            {
                await gameSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, cancellationToken);
                result = await gameSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            }
            await gameSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, cancellationToken);
        }
    }
}
