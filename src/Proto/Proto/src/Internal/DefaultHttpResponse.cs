// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Contoso.GameNetCore.Proto.Features;
using Microsoft.Net.Proto.Headers;

namespace Contoso.GameNetCore.Proto.Internal
{
    public sealed class DefaultProtoResponse : ProtoResponse
    {
        // Lambdas hoisted to static readonly fields to improve inlining https://github.com/dotnet/roslyn/issues/13624
        private readonly static Func<IFeatureCollection, IProtoResponseFeature> _nullResponseFeature = f => null;
        private readonly static Func<IFeatureCollection, IProtoResponseStartFeature> _nullResponseStartFeature = f => null;
        private readonly static Func<IFeatureCollection, IResponseCookiesFeature> _newResponseCookiesFeature = f => new ResponseCookiesFeature(f);
        private readonly static Func<ProtoContext, IResponseBodyPipeFeature> _newResponseBodyPipeFeature = context => new ResponseBodyPipeFeature(context);

        private readonly DefaultProtoContext _context;
        private FeatureReferences<FeatureInterfaces> _features;

        public DefaultProtoResponse(DefaultProtoContext context)
        {
            _context = context;
            _features.Initalize(context.Features);
        }

        public void Initialize()
        {
            _features.Initalize(_context.Features);
        }

        public void Initialize(int revision)
        {
            _features.Initalize(_context.Features, revision);
        }

        public void Uninitialize()
        {
            _features = default;
        }

        private IProtoResponseFeature ProtoResponseFeature =>
            _features.Fetch(ref _features.Cache.Response, _nullResponseFeature);

        private IProtoResponseStartFeature ProtoResponseStartFeature =>
            _features.Fetch(ref _features.Cache.ResponseStart, _nullResponseStartFeature);

        private IResponseCookiesFeature ResponseCookiesFeature =>
            _features.Fetch(ref _features.Cache.Cookies, _newResponseCookiesFeature);

        private IResponseBodyPipeFeature ResponseBodyPipeFeature =>
            _features.Fetch(ref _features.Cache.BodyPipe, this.ProtoContext, _newResponseBodyPipeFeature);

        public override ProtoContext ProtoContext { get { return _context; } }

        public override int StatusCode
        {
            get { return ProtoResponseFeature.StatusCode; }
            set { ProtoResponseFeature.StatusCode = value; }
        }

        public override IHeaderDictionary Headers
        {
            get { return ProtoResponseFeature.Headers; }
        }

        public override Stream Body
        {
            get { return ProtoResponseFeature.Body; }
            set { ProtoResponseFeature.Body = value; }
        }

        public override long? ContentLength
        {
            get { return Headers.ContentLength; }
            set { Headers.ContentLength = value; }
        }

        public override string ContentType
        {
            get
            {
                return Headers[HeaderNames.ContentType];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    ProtoResponseFeature.Headers.Remove(HeaderNames.ContentType);
                }
                else
                {
                    ProtoResponseFeature.Headers[HeaderNames.ContentType] = value;
                }
            }
        }

        public override IResponseCookies Cookies
        {
            get { return ResponseCookiesFeature.Cookies; }
        }

        public override bool HasStarted
        {
            get { return ProtoResponseFeature.HasStarted; }
        }

        public override PipeWriter BodyWriter
        {
            get { return ResponseBodyPipeFeature.Writer; }
            set { ResponseBodyPipeFeature.Writer = value; }
        }

        public override void OnStarting(Func<object, Task> callback, object state)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            ProtoResponseFeature.OnStarting(callback, state);
        }

        public override void OnCompleted(Func<object, Task> callback, object state)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            ProtoResponseFeature.OnCompleted(callback, state);
        }

        public override void Redirect(string location, bool permanent)
        {
            if (permanent)
            {
                ProtoResponseFeature.StatusCode = 301;
            }
            else
            {
                ProtoResponseFeature.StatusCode = 302;
            }

            Headers[HeaderNames.Location] = location;
        }

        public override Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (HasStarted)
            {
                return Task.CompletedTask;
            }

            if (ProtoResponseStartFeature == null)
            {
                return ProtoResponseFeature.Body.FlushAsync(cancellationToken);
            }

            return ProtoResponseStartFeature.StartAsync(cancellationToken);
        }

        struct FeatureInterfaces
        {
            public IProtoResponseFeature Response;
            public IResponseCookiesFeature Cookies;
            public IResponseBodyPipeFeature BodyPipe;
            public IProtoResponseStartFeature ResponseStart;
        }
    }
}
