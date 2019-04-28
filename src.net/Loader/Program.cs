using Contoso.GameNetCore;
using Contoso.GameNetCore.Hosting;

namespace Loader
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateGameHostBuilder(args).Build().Run();
        }

        public static IGameHostBuilder CreateGameHostBuilder(string[] args) =>
            GameHost.CreateDefaultBuilder(args)
                .UseClient()
                .UseSimple()
                .UseStartup<Startup>();
    }
}