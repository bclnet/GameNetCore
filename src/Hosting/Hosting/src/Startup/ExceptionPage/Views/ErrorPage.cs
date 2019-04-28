namespace Contoso.GameNetCore.Hosting.Views
{
    using Contoso.GameNetCore.Proto;
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
