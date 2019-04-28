// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Contoso.GameNetCore.Proto.Features;
using Contoso.GameNetCore.Proto.Features.Authentication;
using Contoso.GameNetCore.Proto.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.GameNetCore.Proto
{
    public sealed class DefaultProtoContext : ProtoContext
    {
        // Lambdas hoisted to static readonly fields to improve inlining https://github.com/dotnet/roslyn/issues/13624
        private readonly static Func<IFeatureCollection, IItemsFeature> _newItemsFeature = f => new ItemsFeature();
        private readonly static Func<DefaultProtoContext, IServiceProvidersFeature> _newServiceProvidersFeature = context => new RequestServicesFeature(context, context.ServiceScopeFactory);
        private readonly static Func<IFeatureCollection, IProtoAuthenticationFeature> _newProtoAuthenticationFeature = f => new ProtoAuthenticationFeature();
        private readonly static Func<IFeatureCollection, IProtoRequestLifetimeFeature> _newProtoRequestLifetimeFeature = f => new ProtoRequestLifetimeFeature();
        private readonly static Func<IFeatureCollection, ISessionFeature> _newSessionFeature = f => new DefaultSessionFeature();
        private readonly static Func<IFeatureCollection, ISessionFeature> _nullSessionFeature = f => null;
        private readonly static Func<IFeatureCollection, IProtoRequestIdentifierFeature> _newProtoRequestIdentifierFeature = f => new ProtoRequestIdentifierFeature();

        private FeatureReferences<FeatureInterfaces> _features;

        private readonly DefaultProtoRequest _request;
        private readonly DefaultProtoResponse _response;

        private DefaultConnectionInfo _connection;
        private DefaultGameSocketManager _gamesockets;

        public DefaultProtoContext()
            : this(new FeatureCollection())
        {
            Features.Set<IProtoRequestFeature>(new ProtoRequestFeature());
            Features.Set<IProtoResponseFeature>(new ProtoResponseFeature());
        }

        public DefaultProtoContext(IFeatureCollection features)
        {
            _features.Initalize(features);
            _request = new DefaultProtoRequest(this);
            _response = new DefaultProtoResponse(this);
        }

        public void Initialize(IFeatureCollection features)
        {
            var revision = features.Revision;
            _features.Initalize(features, revision);
            _request.Initialize(revision);
            _response.Initialize(revision);
            _connection?.Initialize(features, revision);
            _gamesockets?.Initialize(features, revision);
        }

        public void Uninitialize()
        {
            _features = default;
            _request.Uninitialize();
            _response.Uninitialize();
            _connection?.Uninitialize();
            _gamesockets?.Uninitialize();
        }

        public FormOptions FormOptions { get; set; }

        public IServiceScopeFactory ServiceScopeFactory { get; set; }

        private IItemsFeature ItemsFeature =>
            _features.Fetch(ref _features.Cache.Items, _newItemsFeature);

        private IServiceProvidersFeature ServiceProvidersFeature =>
            _features.Fetch(ref _features.Cache.ServiceProviders, this, _newServiceProvidersFeature);

        private IProtoAuthenticationFeature ProtoAuthenticationFeature =>
            _features.Fetch(ref _features.Cache.Authentication, _newProtoAuthenticationFeature);

        private IProtoRequestLifetimeFeature LifetimeFeature =>
            _features.Fetch(ref _features.Cache.Lifetime, _newProtoRequestLifetimeFeature);

        private ISessionFeature SessionFeature =>
            _features.Fetch(ref _features.Cache.Session, _newSessionFeature);

        private ISessionFeature SessionFeatureOrNull =>
            _features.Fetch(ref _features.Cache.Session, _nullSessionFeature);


        private IProtoRequestIdentifierFeature RequestIdentifierFeature =>
            _features.Fetch(ref _features.Cache.RequestIdentifier, _newProtoRequestIdentifierFeature);

        public override IFeatureCollection Features => _features.Collection;

        public override ProtoRequest Request => _request;

        public override ProtoResponse Response => _response;

        public override ConnectionInfo Connection => _connection ?? (_connection = new DefaultConnectionInfo(_features.Collection));

        public override GameSocketManager GameSockets => _gamesockets ?? (_gamesockets = new DefaultGameSocketManager(_features.Collection));

        public override ClaimsPrincipal User
        {
            get
            {
                var user = ProtoAuthenticationFeature.User;
                if (user == null)
                {
                    user = new ClaimsPrincipal(new ClaimsIdentity());
                    ProtoAuthenticationFeature.User = user;
                }
                return user;
            }
            set { ProtoAuthenticationFeature.User = value; }
        }

        public override IDictionary<object, object> Items
        {
            get { return ItemsFeature.Items; }
            set { ItemsFeature.Items = value; }
        }

        public override IServiceProvider RequestServices
        {
            get { return ServiceProvidersFeature.RequestServices; }
            set { ServiceProvidersFeature.RequestServices = value; }
        }

        public override CancellationToken RequestAborted
        {
            get { return LifetimeFeature.RequestAborted; }
            set { LifetimeFeature.RequestAborted = value; }
        }

        public override string TraceIdentifier
        {
            get { return RequestIdentifierFeature.TraceIdentifier; }
            set { RequestIdentifierFeature.TraceIdentifier = value; }
        }

        public override ISession Session
        {
            get
            {
                var feature = SessionFeatureOrNull;
                if (feature == null)
                {
                    throw new InvalidOperationException("Session has not been configured for this application " +
                        "or request.");
                }
                return feature.Session;
            }
            set
            {
                SessionFeature.Session = value;
            }
        }


        public override void Abort()
        {
            LifetimeFeature.Abort();
        }

        struct FeatureInterfaces
        {
            public IItemsFeature Items;
            public IServiceProvidersFeature ServiceProviders;
            public IProtoAuthenticationFeature Authentication;
            public IProtoRequestLifetimeFeature Lifetime;
            public ISessionFeature Session;
            public IProtoRequestIdentifierFeature RequestIdentifier;
        }
    }
}
