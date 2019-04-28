// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.GameSockets;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Proto.Features;
using Contoso.GameNetCore.Proto.Features.Authentication;

namespace Contoso.GameNetCore.Owin
{
    using SendFileFunc = Func<string, long, long?, CancellationToken, Task>;

    public class OwinFeatureCollection :
        IFeatureCollection,
        IProtoRequestFeature,
        IProtoResponseFeature,
        IProtoConnectionFeature,
        IProtoSendFileFeature,
        ITlsConnectionFeature,
        IProtoRequestIdentifierFeature,
        IProtoRequestLifetimeFeature,
        IProtoAuthenticationFeature,
        IProtoGameSocketFeature,
        IOwinEnvironmentFeature
    {
        public IDictionary<string, object> Environment { get; set; }
        private bool _headersSent;

        public OwinFeatureCollection(IDictionary<string, object> environment)
        {
            Environment = environment;
            SupportsGameSockets = true;

            var register = Prop<Action<Action<object>, object>>(OwinConstants.CommonKeys.OnSendingHeaders);
            register?.Invoke(state =>
            {
                var collection = (OwinFeatureCollection)state;
                collection._headersSent = true;
            }, this);
        }

        T Prop<T>(string key)
        {
            object value;
            if (Environment.TryGetValue(key, out value) && value is T)
            {
                return (T)value;
            }
            return default(T);
        }

        void Prop(string key, object value)
        {
            Environment[key] = value;
        }

        string IProtoRequestFeature.Protocol
        {
            get { return Prop<string>(OwinConstants.RequestProtocol); }
            set { Prop(OwinConstants.RequestProtocol, value); }
        }

        string IProtoRequestFeature.Scheme
        {
            get { return Prop<string>(OwinConstants.RequestScheme); }
            set { Prop(OwinConstants.RequestScheme, value); }
        }

        string IProtoRequestFeature.Method
        {
            get { return Prop<string>(OwinConstants.RequestMethod); }
            set { Prop(OwinConstants.RequestMethod, value); }
        }

        string IProtoRequestFeature.PathBase
        {
            get { return Prop<string>(OwinConstants.RequestPathBase); }
            set { Prop(OwinConstants.RequestPathBase, value); }
        }

        string IProtoRequestFeature.Path
        {
            get { return Prop<string>(OwinConstants.RequestPath); }
            set { Prop(OwinConstants.RequestPath, value); }
        }

        string IProtoRequestFeature.QueryString
        {
            get { return Utilities.AddQuestionMark(Prop<string>(OwinConstants.RequestQueryString)); }
            set { Prop(OwinConstants.RequestQueryString, Utilities.RemoveQuestionMark(value)); }
        }

        string IProtoRequestFeature.RawTarget
        {
            get { return string.Empty; }
            set { throw new NotSupportedException(); }
        }

        IHeaderDictionary IProtoRequestFeature.Headers
        {
            get { return Utilities.MakeHeaderDictionary(Prop<IDictionary<string, string[]>>(OwinConstants.RequestHeaders)); }
            set { Prop(OwinConstants.RequestHeaders, Utilities.MakeDictionaryStringArray(value)); }
        }

        string IProtoRequestIdentifierFeature.TraceIdentifier
        {
            get { return Prop<string>(OwinConstants.RequestId); }
            set { Prop(OwinConstants.RequestId, value); }
        }

        Stream IProtoRequestFeature.Body
        {
            get { return Prop<Stream>(OwinConstants.RequestBody); }
            set { Prop(OwinConstants.RequestBody, value); }
        }

        int IProtoResponseFeature.StatusCode
        {
            get { return Prop<int>(OwinConstants.ResponseStatusCode); }
            set { Prop(OwinConstants.ResponseStatusCode, value); }
        }

        string IProtoResponseFeature.ReasonPhrase
        {
            get { return Prop<string>(OwinConstants.ResponseReasonPhrase); }
            set { Prop(OwinConstants.ResponseReasonPhrase, value); }
        }

        IHeaderDictionary IProtoResponseFeature.Headers
        {
            get { return Utilities.MakeHeaderDictionary(Prop<IDictionary<string, string[]>>(OwinConstants.ResponseHeaders)); }
            set { Prop(OwinConstants.ResponseHeaders, Utilities.MakeDictionaryStringArray(value)); }
        }

        Stream IProtoResponseFeature.Body
        {
            get { return Prop<Stream>(OwinConstants.ResponseBody); }
            set { Prop(OwinConstants.ResponseBody, value); }
        }

        bool IProtoResponseFeature.HasStarted
        {
            get { return _headersSent; }
        }

        void IProtoResponseFeature.OnStarting(Func<object, Task> callback, object state)
        {
            var register = Prop<Action<Action<object>, object>>(OwinConstants.CommonKeys.OnSendingHeaders);
            if (register == null)
            {
                throw new NotSupportedException(OwinConstants.CommonKeys.OnSendingHeaders);
            }

            // Need to block on the callback since we can't change the OWIN signature to be async
            register(s => callback(s).GetAwaiter().GetResult(), state);
        }

        void IProtoResponseFeature.OnCompleted(Func<object, Task> callback, object state)
        {
            throw new NotSupportedException();
        }

        IPAddress IProtoConnectionFeature.RemoteIpAddress
        {
            get { return IPAddress.Parse(Prop<string>(OwinConstants.CommonKeys.RemoteIpAddress)); }
            set { Prop(OwinConstants.CommonKeys.RemoteIpAddress, value.ToString()); }
        }

        IPAddress IProtoConnectionFeature.LocalIpAddress
        {
            get { return IPAddress.Parse(Prop<string>(OwinConstants.CommonKeys.LocalIpAddress)); }
            set { Prop(OwinConstants.CommonKeys.LocalIpAddress, value.ToString()); }
        }

        int IProtoConnectionFeature.RemotePort
        {
            get { return int.Parse(Prop<string>(OwinConstants.CommonKeys.RemotePort)); }
            set { Prop(OwinConstants.CommonKeys.RemotePort, value.ToString(CultureInfo.InvariantCulture)); }
        }

        int IProtoConnectionFeature.LocalPort
        {
            get { return int.Parse(Prop<string>(OwinConstants.CommonKeys.LocalPort)); }
            set { Prop(OwinConstants.CommonKeys.LocalPort, value.ToString(CultureInfo.InvariantCulture)); }
        }

        string IProtoConnectionFeature.ConnectionId
        {
            get { return Prop<string>(OwinConstants.CommonKeys.ConnectionId); }
            set { Prop(OwinConstants.CommonKeys.ConnectionId, value); }
        }

        private bool SupportsSendFile
        {
            get
            {
                object obj;
                return Environment.TryGetValue(OwinConstants.SendFiles.SendAsync, out obj) && obj != null;
            }
        }

        Task IProtoSendFileFeature.SendFileAsync(string path, long offset, long? length, CancellationToken cancellation)
        {
            object obj;
            if (Environment.TryGetValue(OwinConstants.SendFiles.SendAsync, out obj))
            {
                var func = (SendFileFunc)obj;
                return func(path, offset, length, cancellation);
            }
            throw new NotSupportedException(OwinConstants.SendFiles.SendAsync);
        }

        private bool SupportsClientCerts
        {
            get
            {
                object obj;
                if (string.Equals("https", ((IProtoRequestFeature)this).Scheme, StringComparison.OrdinalIgnoreCase)
                    && (Environment.TryGetValue(OwinConstants.CommonKeys.LoadClientCertAsync, out obj)
                        || Environment.TryGetValue(OwinConstants.CommonKeys.ClientCertificate, out obj))
                    && obj != null)
                {
                    return true;
                }
                return false;
            }
        }

        X509Certificate2 ITlsConnectionFeature.ClientCertificate
        {
            get { return Prop<X509Certificate2>(OwinConstants.CommonKeys.ClientCertificate); }
            set { Prop(OwinConstants.CommonKeys.ClientCertificate, value); }
        }

        async Task<X509Certificate2> ITlsConnectionFeature.GetClientCertificateAsync(CancellationToken cancellationToken)
        {
            var loadAsync = Prop<Func<Task>>(OwinConstants.CommonKeys.LoadClientCertAsync);
            if (loadAsync != null)
            {
                await loadAsync();
            }
            return Prop<X509Certificate2>(OwinConstants.CommonKeys.ClientCertificate);
        }

        CancellationToken IProtoRequestLifetimeFeature.RequestAborted
        {
            get { return Prop<CancellationToken>(OwinConstants.CallCancelled); }
            set { Prop(OwinConstants.CallCancelled, value); }
        }

        void IProtoRequestLifetimeFeature.Abort()
        {
            throw new NotImplementedException();
        }

        ClaimsPrincipal IProtoAuthenticationFeature.User
        {
            get
            {
                return Prop<ClaimsPrincipal>(OwinConstants.RequestUser)
                    ?? Utilities.MakeClaimsPrincipal(Prop<IPrincipal>(OwinConstants.Security.User));
            }
            set
            {
                Prop(OwinConstants.RequestUser, value);
                Prop(OwinConstants.Security.User, value);
            }
        }

        /// <summary>
        /// Gets or sets if the underlying server supports GameSockets. This is enabled by default.
        /// The value should be consistent across requests.
        /// </summary>
        public bool SupportsGameSockets { get; set; }

        bool IProtoGameSocketFeature.IsGameSocketRequest
        {
            get
            {
                object obj;
                return Environment.TryGetValue(OwinConstants.GameSocket.AcceptAlt, out obj);
            }
        }

        Task<GameSocket> IProtoGameSocketFeature.AcceptAsync(GameSocketAcceptContext context)
        {
            object obj;
            if (!Environment.TryGetValue(OwinConstants.GameSocket.AcceptAlt, out obj))
            {
                throw new NotSupportedException("GameSockets are not supported"); // TODO: LOC
            }
            var accept = (Func<GameSocketAcceptContext, Task<GameSocket>>)obj;
            return accept(context);
        }

        // IFeatureCollection

        public int Revision
        {
            get { return 0; } // Not modifiable
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public object this[Type key]
        {
            get { return Get(key); }
            set { throw new NotSupportedException(); }
        }

        private bool SupportsInterface(Type key)
        {
            // Does this type implement the requested interface?
            if (key.GetTypeInfo().IsAssignableFrom(GetType().GetTypeInfo()))
            {
                // Check for conditional features
                if (key == typeof(IProtoSendFileFeature))
                {
                    return SupportsSendFile;
                }
                else if (key == typeof(ITlsConnectionFeature))
                {
                    return SupportsClientCerts;
                }
                else if (key == typeof(IProtoGameSocketFeature))
                {
                    return SupportsGameSockets;
                }

                // The rest of the features are always supported.
                return true;
            }
            return false;
        }

        public object Get(Type key)
        {
            if (SupportsInterface(key))
            {
                return this;
            }
            return null;
        }

        public void Set(Type key, object value)
        {
            throw new NotSupportedException();
        }

        public TFeature Get<TFeature>()
        {
            return (TFeature)this[typeof(TFeature)];
        }

        public void Set<TFeature>(TFeature instance)
        {
            this[typeof(TFeature)] = instance;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            yield return new KeyValuePair<Type, object>(typeof(IProtoRequestFeature), this);
            yield return new KeyValuePair<Type, object>(typeof(IProtoResponseFeature), this);
            yield return new KeyValuePair<Type, object>(typeof(IProtoConnectionFeature), this);
            yield return new KeyValuePair<Type, object>(typeof(IProtoRequestIdentifierFeature), this);
            yield return new KeyValuePair<Type, object>(typeof(IProtoRequestLifetimeFeature), this);
            yield return new KeyValuePair<Type, object>(typeof(IProtoAuthenticationFeature), this);
            yield return new KeyValuePair<Type, object>(typeof(IOwinEnvironmentFeature), this);

            // Check for conditional features
            if (SupportsSendFile)
            {
                yield return new KeyValuePair<Type, object>(typeof(IProtoSendFileFeature), this);
            }
            if (SupportsClientCerts)
            {
                yield return new KeyValuePair<Type, object>(typeof(ITlsConnectionFeature), this);
            }
            if (SupportsGameSockets)
            {
                yield return new KeyValuePair<Type, object>(typeof(IProtoGameSocketFeature), this);
            }
        }

        public void Dispose()
        {
        }
    }
}

