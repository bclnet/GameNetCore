using Contoso.GameNetCore.Hosting.Server.Features;
using System.Linq;

namespace Contoso.GameNetCore.Hosting
{
    public static class IWebHostExtensions
    {
        public static string GetAddress(this IWebHost host)
        {
            return host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.First();
        }
    }
}
