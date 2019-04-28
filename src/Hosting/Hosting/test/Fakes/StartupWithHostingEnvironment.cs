using Contoso.GameNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Contoso.GameNetCore.Hosting.Tests.Fakes
{
    public class StartupWithHostingEnvironment
    {
        public StartupWithHostingEnvironment(IHostEnvironment env)
        {
            env.EnvironmentName = "Changed";
        }

        public void Configure(IApplicationBuilder app)
        {

        }
    }
}
