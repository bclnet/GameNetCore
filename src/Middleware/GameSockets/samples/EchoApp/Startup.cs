using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.GameSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Hosting;
using Contoso.GameNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EchoApp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => builder.AddConsole());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseGameSockets();

            app.Use(async (context, next) =>
            {
                if (context.GameSockets.IsGameSocketRequest)
                {
                    var gameSocket = await context.GameSockets.AcceptGameSocketAsync();
                    await Echo(context, gameSocket, loggerFactory.CreateLogger("Echo"));
                }
                else
                {
                    await next();
                }
            });

            app.UseFileServer();
        }

        private async Task Echo(HttpContext context, GameSocket gameSocket, ILogger logger)
        {
            var buffer = new byte[1024 * 4];
            var result = await gameSocket.ReceiveAsync(buffer.AsMemory(), CancellationToken.None);
            LogFrame(logger, gameSocket, result, buffer);
            while (result.MessageType != GameSocketMessageType.Close)
            {
                // If the client send "ServerClose", then they want a server-originated close to occur
                string content = "<<binary>>";
                if (result.MessageType == GameSocketMessageType.Text)
                {
                    content = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (content.Equals("ServerClose"))
                    {
                        await gameSocket.CloseAsync(GameSocketCloseStatus.NormalClosure, "Closing from Server", CancellationToken.None);
                        logger.LogDebug($"Sent Frame Close: {GameSocketCloseStatus.NormalClosure} Closing from Server");
                        return;
                    }
                    else if (content.Equals("ServerAbort"))
                    {
                        context.Abort();
                    }
                }

                await gameSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                logger.LogDebug($"Sent Frame {result.MessageType}: Len={result.Count}, Fin={result.EndOfMessage}: {content}");

                result = await gameSocket.ReceiveAsync(buffer.AsMemory(), CancellationToken.None);
                LogFrame(logger, gameSocket, result, buffer);
            }
            await gameSocket.CloseAsync(gameSocket.CloseStatus.Value, gameSocket.CloseStatusDescription, CancellationToken.None);
        }

        private void LogFrame(ILogger logger, GameSocket gameSocket, ValueGameSocketReceiveResult frame, byte[] buffer)
        {
            var close = frame.MessageType == GameSocketMessageType.Close;
            string message;
            if (close)
            {
                message = $"Close: {gameSocket.CloseStatus.Value} {gameSocket.CloseStatusDescription}";
            }
            else
            {
                string content = "<<binary>>";
                if (frame.MessageType == GameSocketMessageType.Text)
                {
                    content = Encoding.UTF8.GetString(buffer, 0, frame.Count);
                }
                message = $"{frame.MessageType}: Len={frame.Count}, Fin={frame.EndOfMessage}: {content}";
            }
            logger.LogDebug("Received Frame " + message);
        }
    }
}
