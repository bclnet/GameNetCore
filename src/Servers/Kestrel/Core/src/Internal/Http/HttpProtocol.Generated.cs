// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

using Contoso.GameNetCore.Proto.Features;
using Contoso.GameNetCore.Proto.Features.Authentication;
using Contoso.GameNetCore.Server.Kestrel.Core.Features;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto
{
    internal partial class ProtoProtocol : IFeatureCollection
    {
        private static readonly Type IProtoRequestFeatureType = typeof(IProtoRequestFeature);
        private static readonly Type IProtoResponseFeatureType = typeof(IProtoResponseFeature);
        private static readonly Type IResponseBodyPipeFeatureType = typeof(IResponseBodyPipeFeature);
        private static readonly Type IRequestBodyPipeFeatureType = typeof(IRequestBodyPipeFeature);
        private static readonly Type IProtoRequestIdentifierFeatureType = typeof(IProtoRequestIdentifierFeature);
        private static readonly Type IServiceProvidersFeatureType = typeof(IServiceProvidersFeature);
        private static readonly Type IProtoRequestLifetimeFeatureType = typeof(IProtoRequestLifetimeFeature);
        private static readonly Type IProtoConnectionFeatureType = typeof(IProtoConnectionFeature);
        private static readonly Type IProtoAuthenticationFeatureType = typeof(IProtoAuthenticationFeature);
        private static readonly Type IQueryFeatureType = typeof(IQueryFeature);
        private static readonly Type IFormFeatureType = typeof(IFormFeature);
        private static readonly Type IProtoUpgradeFeatureType = typeof(IProtoUpgradeFeature);
        private static readonly Type IProto2StreamIdFeatureType = typeof(IProto2StreamIdFeature);
        private static readonly Type IProtoResponseTrailersFeatureType = typeof(IProtoResponseTrailersFeature);
        private static readonly Type IResponseCookiesFeatureType = typeof(IResponseCookiesFeature);
        private static readonly Type IItemsFeatureType = typeof(IItemsFeature);
        private static readonly Type ITlsConnectionFeatureType = typeof(ITlsConnectionFeature);
        private static readonly Type IProtoGameSocketFeatureType = typeof(IProtoGameSocketFeature);
        private static readonly Type ISessionFeatureType = typeof(ISessionFeature);
        private static readonly Type IProtoMaxRequestBodySizeFeatureType = typeof(IProtoMaxRequestBodySizeFeature);
        private static readonly Type IProtoMinRequestBodyDataRateFeatureType = typeof(IProtoMinRequestBodyDataRateFeature);
        private static readonly Type IProtoMinResponseDataRateFeatureType = typeof(IProtoMinResponseDataRateFeature);
        private static readonly Type IProtoBodyControlFeatureType = typeof(IProtoBodyControlFeature);
        private static readonly Type IProtoResponseStartFeatureType = typeof(IProtoResponseStartFeature);
        private static readonly Type IProtoSendFileFeatureType = typeof(IProtoSendFileFeature);

        private object _currentIProtoRequestFeature;
        private object _currentIProtoResponseFeature;
        private object _currentIResponseBodyPipeFeature;
        private object _currentIRequestBodyPipeFeature;
        private object _currentIProtoRequestIdentifierFeature;
        private object _currentIServiceProvidersFeature;
        private object _currentIProtoRequestLifetimeFeature;
        private object _currentIProtoConnectionFeature;
        private object _currentIProtoAuthenticationFeature;
        private object _currentIQueryFeature;
        private object _currentIFormFeature;
        private object _currentIProtoUpgradeFeature;
        private object _currentIProto2StreamIdFeature;
        private object _currentIProtoResponseTrailersFeature;
        private object _currentIResponseCookiesFeature;
        private object _currentIItemsFeature;
        private object _currentITlsConnectionFeature;
        private object _currentIProtoGameSocketFeature;
        private object _currentISessionFeature;
        private object _currentIProtoMaxRequestBodySizeFeature;
        private object _currentIProtoMinRequestBodyDataRateFeature;
        private object _currentIProtoMinResponseDataRateFeature;
        private object _currentIProtoBodyControlFeature;
        private object _currentIProtoResponseStartFeature;
        private object _currentIProtoSendFileFeature;

        private int _featureRevision;

        private List<KeyValuePair<Type, object>> MaybeExtra;

        private void FastReset()
        {
            _currentIProtoRequestFeature = this;
            _currentIProtoResponseFeature = this;
            _currentIResponseBodyPipeFeature = this;
            _currentIRequestBodyPipeFeature = this;
            _currentIProtoUpgradeFeature = this;
            _currentIProtoRequestIdentifierFeature = this;
            _currentIProtoRequestLifetimeFeature = this;
            _currentIProtoConnectionFeature = this;
            _currentIProtoMaxRequestBodySizeFeature = this;
            _currentIProtoBodyControlFeature = this;
            _currentIProtoResponseStartFeature = this;

            _currentIServiceProvidersFeature = null;
            _currentIProtoAuthenticationFeature = null;
            _currentIQueryFeature = null;
            _currentIFormFeature = null;
            _currentIProto2StreamIdFeature = null;
            _currentIProtoResponseTrailersFeature = null;
            _currentIResponseCookiesFeature = null;
            _currentIItemsFeature = null;
            _currentITlsConnectionFeature = null;
            _currentIProtoGameSocketFeature = null;
            _currentISessionFeature = null;
            _currentIProtoMinRequestBodyDataRateFeature = null;
            _currentIProtoMinResponseDataRateFeature = null;
            _currentIProtoSendFileFeature = null;
        }

        // Internal for testing
        internal void ResetFeatureCollection()
        {
            FastReset();
            MaybeExtra?.Clear();
            _featureRevision++;
        }

        private object ExtraFeatureGet(Type key)
        {
            if (MaybeExtra == null)
            {
                return null;
            }
            for (var i = 0; i < MaybeExtra.Count; i++)
            {
                var kv = MaybeExtra[i];
                if (kv.Key == key)
                {
                    return kv.Value;
                }
            }
            return null;
        }

        private void ExtraFeatureSet(Type key, object value)
        {
            if (MaybeExtra == null)
            {
                MaybeExtra = new List<KeyValuePair<Type, object>>(2);
            }

            for (var i = 0; i < MaybeExtra.Count; i++)
            {
                if (MaybeExtra[i].Key == key)
                {
                    MaybeExtra[i] = new KeyValuePair<Type, object>(key, value);
                    return;
                }
            }
            MaybeExtra.Add(new KeyValuePair<Type, object>(key, value));
        }

        bool IFeatureCollection.IsReadOnly => false;

        int IFeatureCollection.Revision => _featureRevision;

        object IFeatureCollection.this[Type key]
        {
            get
            {
                object feature = null;
                if (key == IProtoRequestFeatureType)
                {
                    feature = _currentIProtoRequestFeature;
                }
                else if (key == IProtoResponseFeatureType)
                {
                    feature = _currentIProtoResponseFeature;
                }
                else if (key == IResponseBodyPipeFeatureType)
                {
                    feature = _currentIResponseBodyPipeFeature;
                }
                else if (key == IRequestBodyPipeFeatureType)
                {
                    feature = _currentIRequestBodyPipeFeature;
                }
                else if (key == IProtoRequestIdentifierFeatureType)
                {
                    feature = _currentIProtoRequestIdentifierFeature;
                }
                else if (key == IServiceProvidersFeatureType)
                {
                    feature = _currentIServiceProvidersFeature;
                }
                else if (key == IProtoRequestLifetimeFeatureType)
                {
                    feature = _currentIProtoRequestLifetimeFeature;
                }
                else if (key == IProtoConnectionFeatureType)
                {
                    feature = _currentIProtoConnectionFeature;
                }
                else if (key == IProtoAuthenticationFeatureType)
                {
                    feature = _currentIProtoAuthenticationFeature;
                }
                else if (key == IQueryFeatureType)
                {
                    feature = _currentIQueryFeature;
                }
                else if (key == IFormFeatureType)
                {
                    feature = _currentIFormFeature;
                }
                else if (key == IProtoUpgradeFeatureType)
                {
                    feature = _currentIProtoUpgradeFeature;
                }
                else if (key == IProto2StreamIdFeatureType)
                {
                    feature = _currentIProto2StreamIdFeature;
                }
                else if (key == IProtoResponseTrailersFeatureType)
                {
                    feature = _currentIProtoResponseTrailersFeature;
                }
                else if (key == IResponseCookiesFeatureType)
                {
                    feature = _currentIResponseCookiesFeature;
                }
                else if (key == IItemsFeatureType)
                {
                    feature = _currentIItemsFeature;
                }
                else if (key == ITlsConnectionFeatureType)
                {
                    feature = _currentITlsConnectionFeature;
                }
                else if (key == IProtoGameSocketFeatureType)
                {
                    feature = _currentIProtoGameSocketFeature;
                }
                else if (key == ISessionFeatureType)
                {
                    feature = _currentISessionFeature;
                }
                else if (key == IProtoMaxRequestBodySizeFeatureType)
                {
                    feature = _currentIProtoMaxRequestBodySizeFeature;
                }
                else if (key == IProtoMinRequestBodyDataRateFeatureType)
                {
                    feature = _currentIProtoMinRequestBodyDataRateFeature;
                }
                else if (key == IProtoMinResponseDataRateFeatureType)
                {
                    feature = _currentIProtoMinResponseDataRateFeature;
                }
                else if (key == IProtoBodyControlFeatureType)
                {
                    feature = _currentIProtoBodyControlFeature;
                }
                else if (key == IProtoResponseStartFeatureType)
                {
                    feature = _currentIProtoResponseStartFeature;
                }
                else if (key == IProtoSendFileFeatureType)
                {
                    feature = _currentIProtoSendFileFeature;
                }
                else if (MaybeExtra != null)
                {
                    feature = ExtraFeatureGet(key);
                }

                return feature ?? ConnectionFeatures[key];
            }

            set
            {
                _featureRevision++;

                if (key == IProtoRequestFeatureType)
                {
                    _currentIProtoRequestFeature = value;
                }
                else if (key == IProtoResponseFeatureType)
                {
                    _currentIProtoResponseFeature = value;
                }
                else if (key == IResponseBodyPipeFeatureType)
                {
                    _currentIResponseBodyPipeFeature = value;
                }
                else if (key == IRequestBodyPipeFeatureType)
                {
                    _currentIRequestBodyPipeFeature = value;
                }
                else if (key == IProtoRequestIdentifierFeatureType)
                {
                    _currentIProtoRequestIdentifierFeature = value;
                }
                else if (key == IServiceProvidersFeatureType)
                {
                    _currentIServiceProvidersFeature = value;
                }
                else if (key == IProtoRequestLifetimeFeatureType)
                {
                    _currentIProtoRequestLifetimeFeature = value;
                }
                else if (key == IProtoConnectionFeatureType)
                {
                    _currentIProtoConnectionFeature = value;
                }
                else if (key == IProtoAuthenticationFeatureType)
                {
                    _currentIProtoAuthenticationFeature = value;
                }
                else if (key == IQueryFeatureType)
                {
                    _currentIQueryFeature = value;
                }
                else if (key == IFormFeatureType)
                {
                    _currentIFormFeature = value;
                }
                else if (key == IProtoUpgradeFeatureType)
                {
                    _currentIProtoUpgradeFeature = value;
                }
                else if (key == IProto2StreamIdFeatureType)
                {
                    _currentIProto2StreamIdFeature = value;
                }
                else if (key == IProtoResponseTrailersFeatureType)
                {
                    _currentIProtoResponseTrailersFeature = value;
                }
                else if (key == IResponseCookiesFeatureType)
                {
                    _currentIResponseCookiesFeature = value;
                }
                else if (key == IItemsFeatureType)
                {
                    _currentIItemsFeature = value;
                }
                else if (key == ITlsConnectionFeatureType)
                {
                    _currentITlsConnectionFeature = value;
                }
                else if (key == IProtoGameSocketFeatureType)
                {
                    _currentIProtoGameSocketFeature = value;
                }
                else if (key == ISessionFeatureType)
                {
                    _currentISessionFeature = value;
                }
                else if (key == IProtoMaxRequestBodySizeFeatureType)
                {
                    _currentIProtoMaxRequestBodySizeFeature = value;
                }
                else if (key == IProtoMinRequestBodyDataRateFeatureType)
                {
                    _currentIProtoMinRequestBodyDataRateFeature = value;
                }
                else if (key == IProtoMinResponseDataRateFeatureType)
                {
                    _currentIProtoMinResponseDataRateFeature = value;
                }
                else if (key == IProtoBodyControlFeatureType)
                {
                    _currentIProtoBodyControlFeature = value;
                }
                else if (key == IProtoResponseStartFeatureType)
                {
                    _currentIProtoResponseStartFeature = value;
                }
                else if (key == IProtoSendFileFeatureType)
                {
                    _currentIProtoSendFileFeature = value;
                }
                else
                {
                    ExtraFeatureSet(key, value);
                }
            }
        }

        TFeature IFeatureCollection.Get<TFeature>()
        {
            TFeature feature = default;
            if (typeof(TFeature) == typeof(IProtoRequestFeature))
            {
                feature = (TFeature)_currentIProtoRequestFeature;
            }
            else if (typeof(TFeature) == typeof(IProtoResponseFeature))
            {
                feature = (TFeature)_currentIProtoResponseFeature;
            }
            else if (typeof(TFeature) == typeof(IResponseBodyPipeFeature))
            {
                feature = (TFeature)_currentIResponseBodyPipeFeature;
            }
            else if (typeof(TFeature) == typeof(IRequestBodyPipeFeature))
            {
                feature = (TFeature)_currentIRequestBodyPipeFeature;
            }
            else if (typeof(TFeature) == typeof(IProtoRequestIdentifierFeature))
            {
                feature = (TFeature)_currentIProtoRequestIdentifierFeature;
            }
            else if (typeof(TFeature) == typeof(IServiceProvidersFeature))
            {
                feature = (TFeature)_currentIServiceProvidersFeature;
            }
            else if (typeof(TFeature) == typeof(IProtoRequestLifetimeFeature))
            {
                feature = (TFeature)_currentIProtoRequestLifetimeFeature;
            }
            else if (typeof(TFeature) == typeof(IProtoConnectionFeature))
            {
                feature = (TFeature)_currentIProtoConnectionFeature;
            }
            else if (typeof(TFeature) == typeof(IProtoAuthenticationFeature))
            {
                feature = (TFeature)_currentIProtoAuthenticationFeature;
            }
            else if (typeof(TFeature) == typeof(IQueryFeature))
            {
                feature = (TFeature)_currentIQueryFeature;
            }
            else if (typeof(TFeature) == typeof(IFormFeature))
            {
                feature = (TFeature)_currentIFormFeature;
            }
            else if (typeof(TFeature) == typeof(IProtoUpgradeFeature))
            {
                feature = (TFeature)_currentIProtoUpgradeFeature;
            }
            else if (typeof(TFeature) == typeof(IProto2StreamIdFeature))
            {
                feature = (TFeature)_currentIProto2StreamIdFeature;
            }
            else if (typeof(TFeature) == typeof(IProtoResponseTrailersFeature))
            {
                feature = (TFeature)_currentIProtoResponseTrailersFeature;
            }
            else if (typeof(TFeature) == typeof(IResponseCookiesFeature))
            {
                feature = (TFeature)_currentIResponseCookiesFeature;
            }
            else if (typeof(TFeature) == typeof(IItemsFeature))
            {
                feature = (TFeature)_currentIItemsFeature;
            }
            else if (typeof(TFeature) == typeof(ITlsConnectionFeature))
            {
                feature = (TFeature)_currentITlsConnectionFeature;
            }
            else if (typeof(TFeature) == typeof(IProtoGameSocketFeature))
            {
                feature = (TFeature)_currentIProtoGameSocketFeature;
            }
            else if (typeof(TFeature) == typeof(ISessionFeature))
            {
                feature = (TFeature)_currentISessionFeature;
            }
            else if (typeof(TFeature) == typeof(IProtoMaxRequestBodySizeFeature))
            {
                feature = (TFeature)_currentIProtoMaxRequestBodySizeFeature;
            }
            else if (typeof(TFeature) == typeof(IProtoMinRequestBodyDataRateFeature))
            {
                feature = (TFeature)_currentIProtoMinRequestBodyDataRateFeature;
            }
            else if (typeof(TFeature) == typeof(IProtoMinResponseDataRateFeature))
            {
                feature = (TFeature)_currentIProtoMinResponseDataRateFeature;
            }
            else if (typeof(TFeature) == typeof(IProtoBodyControlFeature))
            {
                feature = (TFeature)_currentIProtoBodyControlFeature;
            }
            else if (typeof(TFeature) == typeof(IProtoResponseStartFeature))
            {
                feature = (TFeature)_currentIProtoResponseStartFeature;
            }
            else if (typeof(TFeature) == typeof(IProtoSendFileFeature))
            {
                feature = (TFeature)_currentIProtoSendFileFeature;
            }
            else if (MaybeExtra != null)
            {
                feature = (TFeature)(ExtraFeatureGet(typeof(TFeature)));
            }

            if (feature == null)
            {
                feature = ConnectionFeatures.Get<TFeature>();
            }

            return feature;
        }

        void IFeatureCollection.Set<TFeature>(TFeature feature)
        {
            _featureRevision++;
            if (typeof(TFeature) == typeof(IProtoRequestFeature))
            {
                _currentIProtoRequestFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IProtoResponseFeature))
            {
                _currentIProtoResponseFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IResponseBodyPipeFeature))
            {
                _currentIResponseBodyPipeFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IRequestBodyPipeFeature))
            {
                _currentIRequestBodyPipeFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IProtoRequestIdentifierFeature))
            {
                _currentIProtoRequestIdentifierFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IServiceProvidersFeature))
            {
                _currentIServiceProvidersFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IProtoRequestLifetimeFeature))
            {
                _currentIProtoRequestLifetimeFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IProtoConnectionFeature))
            {
                _currentIProtoConnectionFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IProtoAuthenticationFeature))
            {
                _currentIProtoAuthenticationFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IQueryFeature))
            {
                _currentIQueryFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IFormFeature))
            {
                _currentIFormFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IProtoUpgradeFeature))
            {
                _currentIProtoUpgradeFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IProto2StreamIdFeature))
            {
                _currentIProto2StreamIdFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IProtoResponseTrailersFeature))
            {
                _currentIProtoResponseTrailersFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IResponseCookiesFeature))
            {
                _currentIResponseCookiesFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IItemsFeature))
            {
                _currentIItemsFeature = feature;
            }
            else if (typeof(TFeature) == typeof(ITlsConnectionFeature))
            {
                _currentITlsConnectionFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IProtoGameSocketFeature))
            {
                _currentIProtoGameSocketFeature = feature;
            }
            else if (typeof(TFeature) == typeof(ISessionFeature))
            {
                _currentISessionFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IProtoMaxRequestBodySizeFeature))
            {
                _currentIProtoMaxRequestBodySizeFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IProtoMinRequestBodyDataRateFeature))
            {
                _currentIProtoMinRequestBodyDataRateFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IProtoMinResponseDataRateFeature))
            {
                _currentIProtoMinResponseDataRateFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IProtoBodyControlFeature))
            {
                _currentIProtoBodyControlFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IProtoResponseStartFeature))
            {
                _currentIProtoResponseStartFeature = feature;
            }
            else if (typeof(TFeature) == typeof(IProtoSendFileFeature))
            {
                _currentIProtoSendFileFeature = feature;
            }
            else
            {
                ExtraFeatureSet(typeof(TFeature), feature);
            }
        }

        private IEnumerable<KeyValuePair<Type, object>> FastEnumerable()
        {
            if (_currentIProtoRequestFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProtoRequestFeatureType, _currentIProtoRequestFeature);
            }
            if (_currentIProtoResponseFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProtoResponseFeatureType, _currentIProtoResponseFeature);
            }
            if (_currentIResponseBodyPipeFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IResponseBodyPipeFeatureType, _currentIResponseBodyPipeFeature);
            }
            if (_currentIRequestBodyPipeFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IRequestBodyPipeFeatureType, _currentIRequestBodyPipeFeature);
            }
            if (_currentIProtoRequestIdentifierFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProtoRequestIdentifierFeatureType, _currentIProtoRequestIdentifierFeature);
            }
            if (_currentIServiceProvidersFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IServiceProvidersFeatureType, _currentIServiceProvidersFeature);
            }
            if (_currentIProtoRequestLifetimeFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProtoRequestLifetimeFeatureType, _currentIProtoRequestLifetimeFeature);
            }
            if (_currentIProtoConnectionFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProtoConnectionFeatureType, _currentIProtoConnectionFeature);
            }
            if (_currentIProtoAuthenticationFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProtoAuthenticationFeatureType, _currentIProtoAuthenticationFeature);
            }
            if (_currentIQueryFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IQueryFeatureType, _currentIQueryFeature);
            }
            if (_currentIFormFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IFormFeatureType, _currentIFormFeature);
            }
            if (_currentIProtoUpgradeFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProtoUpgradeFeatureType, _currentIProtoUpgradeFeature);
            }
            if (_currentIProto2StreamIdFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProto2StreamIdFeatureType, _currentIProto2StreamIdFeature);
            }
            if (_currentIProtoResponseTrailersFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProtoResponseTrailersFeatureType, _currentIProtoResponseTrailersFeature);
            }
            if (_currentIResponseCookiesFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IResponseCookiesFeatureType, _currentIResponseCookiesFeature);
            }
            if (_currentIItemsFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IItemsFeatureType, _currentIItemsFeature);
            }
            if (_currentITlsConnectionFeature != null)
            {
                yield return new KeyValuePair<Type, object>(ITlsConnectionFeatureType, _currentITlsConnectionFeature);
            }
            if (_currentIProtoGameSocketFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProtoGameSocketFeatureType, _currentIProtoGameSocketFeature);
            }
            if (_currentISessionFeature != null)
            {
                yield return new KeyValuePair<Type, object>(ISessionFeatureType, _currentISessionFeature);
            }
            if (_currentIProtoMaxRequestBodySizeFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProtoMaxRequestBodySizeFeatureType, _currentIProtoMaxRequestBodySizeFeature);
            }
            if (_currentIProtoMinRequestBodyDataRateFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProtoMinRequestBodyDataRateFeatureType, _currentIProtoMinRequestBodyDataRateFeature);
            }
            if (_currentIProtoMinResponseDataRateFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProtoMinResponseDataRateFeatureType, _currentIProtoMinResponseDataRateFeature);
            }
            if (_currentIProtoBodyControlFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProtoBodyControlFeatureType, _currentIProtoBodyControlFeature);
            }
            if (_currentIProtoResponseStartFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProtoResponseStartFeatureType, _currentIProtoResponseStartFeature);
            }
            if (_currentIProtoSendFileFeature != null)
            {
                yield return new KeyValuePair<Type, object>(IProtoSendFileFeatureType, _currentIProtoSendFileFeature);
            }

            if (MaybeExtra != null)
            {
                foreach (var item in MaybeExtra)
                {
                    yield return item;
                }
            }
        }

        IEnumerator<KeyValuePair<Type, object>> IEnumerable<KeyValuePair<Type, object>>.GetEnumerator() => FastEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => FastEnumerable().GetEnumerator();
    }
}
