// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.GameSockets;
using System.Threading.Tasks;
using Contoso.GameNetCore.Proto.Features;
using Microsoft.Net.Proto.Headers;

namespace Contoso.GameNetCore.Proto.Internal
{
    public sealed class DefaultGameSocketManager : GameSocketManager
    {
        // Lambdas hoisted to static readonly fields to improve inlining https://github.com/dotnet/roslyn/issues/13624
        private readonly static Func<IFeatureCollection, IProtoRequestFeature> _nullRequestFeature = f => null;
        private readonly static Func<IFeatureCollection, IProtoGameSocketFeature> _nullGameSocketFeature = f => null;

        private FeatureReferences<FeatureInterfaces> _features;

        public DefaultGameSocketManager(IFeatureCollection features)
        {
            Initialize(features);
        }

        public void Initialize(IFeatureCollection features)
        {
            _features.Initalize(features);
        }

        public void Initialize(IFeatureCollection features, int revision)
        {
            _features.Initalize(features, revision);
        }

        public void Uninitialize()
        {
            _features = default;
        }

        private IProtoRequestFeature ProtoRequestFeature =>
            _features.Fetch(ref _features.Cache.Request, _nullRequestFeature);

        private IProtoGameSocketFeature GameSocketFeature =>
            _features.Fetch(ref _features.Cache.GameSockets, _nullGameSocketFeature);

        public override bool IsGameSocketRequest
        {
            get
            {
                return GameSocketFeature != null && GameSocketFeature.IsGameSocketRequest;
            }
        }

        public override IList<string> GameSocketRequestedProtocols
        {
            get
            {
                return ParsingHelpers.GetHeaderSplit(ProtoRequestFeature.Headers, HeaderNames.GameSocketSubProtocols);
            }
        }

        public override Task<GameSocket> AcceptGameSocketAsync(string subProtocol)
        {
            if (GameSocketFeature == null)
            {
                throw new NotSupportedException("GameSockets are not supported");
            }
            return GameSocketFeature.AcceptAsync(new GameSocketAcceptContext() { SubProtocol = subProtocol });
        }

        struct FeatureInterfaces
        {
            public IProtoRequestFeature Request;
            public IProtoGameSocketFeature GameSockets;
        }
    }
}
