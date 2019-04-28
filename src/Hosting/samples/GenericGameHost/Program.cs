using System.Threading.Tasks;
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Contoso.GameNetCore.Hosting;

namespace GenericWebHost
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHost(builder =>
                {
                    builder.UseKestrel()
                    .Configure(app =>
                    {
                        app.Run(async (context) =>
                        {
                            await context.Response.WriteAsync("Hello World!");
                        });
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}
