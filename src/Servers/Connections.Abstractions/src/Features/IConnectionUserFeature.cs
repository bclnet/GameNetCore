using System.Security.Claims;

namespace Contoso.GameNetCore.Connections.Features
{
    public interface IConnectionUserFeature
    {
        ClaimsPrincipal User { get; set; }
    }
}