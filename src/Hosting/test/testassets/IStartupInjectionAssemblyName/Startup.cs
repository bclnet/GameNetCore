
using System;
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Contoso.GameNetCore.Http;

namespace IStartupInjectionAssemblyName
{
    public class Startup : IStartup
    {
        public void Configure(IApplicationBuilder app)
        {
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider();
        }
    }
}
