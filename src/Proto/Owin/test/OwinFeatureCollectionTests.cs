// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Proto.Features;
using Xunit;

namespace Contoso.GameNetCore.Owin
{
    public class OwinProtoEnvironmentTests
    {
        private T Get<T>(IFeatureCollection features)
        {
            return (T)features[typeof(T)];
        }

        private T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) ? (T)value : default(T);
        }

        [Fact]
        public void OwinProtoEnvironmentCanBeCreated()
        {
            var env = new Dictionary<string, object>
            {
                { "owin.RequestMethod", ProtoMethods.Post },
                { "owin.RequestPath", "/path" },
                { "owin.RequestPathBase", "/pathBase" },
                { "owin.RequestQueryString", "name=value" },
            };
            var features = new OwinFeatureCollection(env);

            var requestFeature = Get<IProtoRequestFeature>(features);
            Assert.Equal(requestFeature.Method, ProtoMethods.Post);
            Assert.Equal("/path", requestFeature.Path);
            Assert.Equal("/pathBase", requestFeature.PathBase);
            Assert.Equal("?name=value", requestFeature.QueryString);
        }

        [Fact]
        public void OwinProtoEnvironmentCanBeModified()
        {
            var env = new Dictionary<string, object>
            {
                { "owin.RequestMethod", ProtoMethods.Post },
                { "owin.RequestPath", "/path" },
                { "owin.RequestPathBase", "/pathBase" },
                { "owin.RequestQueryString", "name=value" },
            };
            var features = new OwinFeatureCollection(env);

            var requestFeature = Get<IProtoRequestFeature>(features);
            requestFeature.Method = ProtoMethods.Get;
            requestFeature.Path = "/path2";
            requestFeature.PathBase = "/pathBase2";
            requestFeature.QueryString = "?name=value2";

            Assert.Equal(ProtoMethods.Get, Get<string>(env, "owin.RequestMethod"));
            Assert.Equal("/path2", Get<string>(env, "owin.RequestPath"));
            Assert.Equal("/pathBase2", Get<string>(env, "owin.RequestPathBase"));
            Assert.Equal("name=value2", Get<string>(env, "owin.RequestQueryString"));
        }
    }
}

