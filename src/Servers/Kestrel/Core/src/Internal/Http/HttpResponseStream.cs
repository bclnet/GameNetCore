// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO.Pipelines;
using Contoso.GameNetCore.Proto.Features;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto
{
    internal class ProtoResponseStream : WriteOnlyPipeStream
    {
        private readonly ProtoResponsePipeWriter _pipeWriter;
        private readonly IProtoBodyControlFeature _bodyControl;

        public ProtoResponseStream(IProtoBodyControlFeature bodyControl, ProtoResponsePipeWriter pipeWriter)
            : base(pipeWriter)
        {
            _bodyControl = bodyControl;
            _pipeWriter = pipeWriter;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_bodyControl.AllowSynchronousIO)
            {
                throw new InvalidOperationException(CoreStrings.SynchronousWritesDisallowed);
            }

            base.Write(buffer, offset, count);
        }

        public override void Flush()
        {
            if (!_bodyControl.AllowSynchronousIO)
            {
                throw new InvalidOperationException(CoreStrings.SynchronousWritesDisallowed);
            }

            base.Flush();
        }
    }
}
