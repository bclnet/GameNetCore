// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Proto.Features;
using Contoso.GameNetCore.Server.Kestrel.Core.Features;
using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto2
{
    internal partial class Proto2Stream : IProto2StreamIdFeature, IProtoResponseTrailersFeature
    {
        internal ProtoResponseTrailers Trailers { get; set; }
        private IHeaderDictionary _userTrailers;

        IHeaderDictionary IProtoResponseTrailersFeature.Trailers
        {
            get
            {
                if (Trailers == null)
                {
                    Trailers = new ProtoResponseTrailers();
                }
                return _userTrailers ?? Trailers;
            }
            set
            {
                _userTrailers = value;
            }
        }

        int IProto2StreamIdFeature.StreamId => _context.StreamId;
    }
}
