using System;
using System.Collections.Generic;
using System.Text;
using Contoso.GameNetCore.Hosting.Server;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    internal class Proto2Stream<TContext> : Proto2Stream
    {
        private readonly IProtoApplication<TContext> _application;

        public Proto2Stream(IProtoApplication<TContext> application, Proto2StreamContext context) : base(context)
        {
            _application = application;
        }

        public override void Execute()
        {
            // REVIEW: Should we store this in a field for easy debugging?
            _ = ProcessRequestsAsync(_application);
        }
    }
}
