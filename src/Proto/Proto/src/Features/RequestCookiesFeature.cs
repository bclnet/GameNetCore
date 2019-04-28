// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Contoso.GameNetCore.Proto.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Proto.Headers;

namespace Contoso.GameNetCore.Proto.Features
{
    public class RequestCookiesFeature : IRequestCookiesFeature
    {
        // Lambda hoisted to static readonly field to improve inlining https://github.com/dotnet/roslyn/issues/13624
        private readonly static Func<IFeatureCollection, IProtoRequestFeature> _nullRequestFeature = f => null;

        private FeatureReferences<IProtoRequestFeature> _features;
        private StringValues _original;
        private IRequestCookieCollection _parsedValues;

        public RequestCookiesFeature(IRequestCookieCollection cookies)
        {
            if (cookies == null)
            {
                throw new ArgumentNullException(nameof(cookies));
            }

            _parsedValues = cookies;
        }

        public RequestCookiesFeature(IFeatureCollection features)
        {
            if (features == null)
            {
                throw new ArgumentNullException(nameof(features));
            }

            _features.Initalize(features);
        }

        private IProtoRequestFeature ProtoRequestFeature =>
            _features.Fetch(ref _features.Cache, _nullRequestFeature);

        public IRequestCookieCollection Cookies
        {
            get
            {
                if (_features.Collection == null)
                {
                    if (_parsedValues == null)
                    {
                        _parsedValues = RequestCookieCollection.Empty;
                    }
                    return _parsedValues;
                }

                var headers = ProtoRequestFeature.Headers;
                StringValues current;
                if (!headers.TryGetValue(HeaderNames.Cookie, out current))
                {
                    current = string.Empty;
                }

                if (_parsedValues == null || _original != current)
                {
                    _original = current;
                    _parsedValues = RequestCookieCollection.Parse(current.ToArray());
                }

                return _parsedValues;
            }
            set
            {
                _parsedValues = value;
                _original = StringValues.Empty;
                if (_features.Collection != null)
                {
                    if (_parsedValues == null || _parsedValues.Count == 0)
                    {
                        ProtoRequestFeature.Headers.Remove(HeaderNames.Cookie);
                    }
                    else
                    {
                        var headers = new List<string>();
                        foreach (var pair in _parsedValues)
                        {
                            headers.Add(new CookieHeaderValue(pair.Key, pair.Value).ToString());
                        }
                        _original = headers.ToArray();
                        ProtoRequestFeature.Headers[HeaderNames.Cookie] = _original;
                    }
                }
            }
        }
    }
}