using System;
using System.Collections.Generic;
using System.Text;

namespace Contoso.GameNetCore.Proto
{
    public interface IDefaultProtoContextContainer
    {
        DefaultProtoContext ProtoContext { get; }
    }
}
