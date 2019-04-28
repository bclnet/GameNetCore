// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Contoso.GameNetCore.Proto.Features;

namespace Contoso.GameNetCore.Proto.Internal
{
    public sealed class DefaultConnectionInfo : ConnectionInfo
    {
        // Lambdas hoisted to static readonly fields to improve inlining https://github.com/dotnet/roslyn/issues/13624
        private readonly static Func<IFeatureCollection, IProtoConnectionFeature> _newProtoConnectionFeature = f => new ProtoConnectionFeature();
        private readonly static Func<IFeatureCollection, ITlsConnectionFeature> _newTlsConnectionFeature = f => new TlsConnectionFeature();

        private FeatureReferences<FeatureInterfaces> _features;

        public DefaultConnectionInfo(IFeatureCollection features)
        {
            Initialize(features);
        }

        public void Initialize( IFeatureCollection features)
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

        private IProtoConnectionFeature ProtoConnectionFeature =>
            _features.Fetch(ref _features.Cache.Connection, _newProtoConnectionFeature);

        private ITlsConnectionFeature TlsConnectionFeature=>
            _features.Fetch(ref _features.Cache.TlsConnection, _newTlsConnectionFeature);

        /// <inheritdoc />
        public override string Id
        {
            get { return ProtoConnectionFeature.ConnectionId; }
            set { ProtoConnectionFeature.ConnectionId = value; }
        }

        public override IPAddress RemoteIpAddress
        {
            get { return ProtoConnectionFeature.RemoteIpAddress; }
            set { ProtoConnectionFeature.RemoteIpAddress = value; }
        }

        public override int RemotePort
        {
            get { return ProtoConnectionFeature.RemotePort; }
            set { ProtoConnectionFeature.RemotePort = value; }
        }

        public override IPAddress LocalIpAddress
        {
            get { return ProtoConnectionFeature.LocalIpAddress; }
            set { ProtoConnectionFeature.LocalIpAddress = value; }
        }

        public override int LocalPort
        {
            get { return ProtoConnectionFeature.LocalPort; }
            set { ProtoConnectionFeature.LocalPort = value; }
        }

        public override X509Certificate2 ClientCertificate
        {
            get { return TlsConnectionFeature.ClientCertificate; }
            set { TlsConnectionFeature.ClientCertificate = value; }
        }

        public override Task<X509Certificate2> GetClientCertificateAsync(CancellationToken cancellationToken = default)
        {
            return TlsConnectionFeature.GetClientCertificateAsync(cancellationToken);
        }

        struct FeatureInterfaces
        {
            public IProtoConnectionFeature Connection;
            public ITlsConnectionFeature TlsConnection;
        }
    }
}
