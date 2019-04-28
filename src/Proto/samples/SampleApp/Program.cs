using System;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Proto.Extensions;

namespace SampleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var query = new QueryBuilder()
            {
                { "hello", "world" }
            }.ToQueryString();

            var uri = UriHelper.BuildAbsolute("http", new HostString("contoso.com"), query: query);

            Console.WriteLine(uri);
        }
    }
}
