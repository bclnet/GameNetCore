namespace Contoso.GameNetCore.Hosting.Views
{
#if NETX
    using ProtoContext = Microsoft.AspNetCore.Http.HttpContext;
#else
    using Contoso.GameNetCore.Proto;
#endif
    using System.Threading.Tasks;

    internal class ErrorPage
    {
        public ErrorPage(ErrorPageModel model) =>
            Model = model;

        public ErrorPageModel Model { get; set; }

        public ErrorPage() { }

#pragma warning disable 1998
        public async Task ExecuteAsync(ProtoContext context)
        {
        }
#pragma warning restore 1998
    }
}
