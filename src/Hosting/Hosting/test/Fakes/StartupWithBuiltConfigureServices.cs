using System;
using Contoso.GameNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.GameNetCore.Hosting.Fakes
{
    public class StartupWithBuiltConfigureServices
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return null;
        }

        public void Configure(IApplicationBuilder app) { }
    }
}
