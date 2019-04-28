using System;
using System.Collections.Generic;
using System.Text;
using Contoso.GameNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.GameNetCore.Hosting.Fakes
{
    public class StartupNoServicesNoInterface
    {
        public void ConfigureServices(IServiceCollection services)
        {

        }

        public void Configure(IApplicationBuilder app)
        {

        }
    }
}
