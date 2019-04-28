// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Contoso.GameNetCore.Proto.Abstractions.Tests
{
    public class CookieBuilderTests
    {
        [Theory]
        [InlineData(CookieSecurePolicy.Always, false, true)]
        [InlineData(CookieSecurePolicy.Always, true, true)]
        [InlineData(CookieSecurePolicy.SameAsRequest, true, true)]
        [InlineData(CookieSecurePolicy.SameAsRequest, false, false)]
        [InlineData(CookieSecurePolicy.None, true, false)]
        [InlineData(CookieSecurePolicy.None, false, false)]
        public void ConfiguresSecurePolicy(CookieSecurePolicy policy, bool requestIsProtos, bool secure)
        {
            var builder = new CookieBuilder
            {
                SecurePolicy = policy
            };
            var context = new DefaultProtoContext();
            context.Request.IsProtos = requestIsProtos;
            var options = builder.Build(context);

            Assert.Equal(secure, options.Secure);
        }

        [Fact]
        public void ComputesExpiration()
        {
            Assert.Null(new CookieBuilder().Build(new DefaultProtoContext()).Expires);

            var now = DateTimeOffset.Now;
            var options = new CookieBuilder { Expiration = TimeSpan.FromHours(1) }.Build(new DefaultProtoContext(), now);
            Assert.Equal(now.AddHours(1), options.Expires);
        }

        [Fact]
        public void ComputesMaxAge()
        {
            Assert.Null(new CookieBuilder().Build(new DefaultProtoContext()).MaxAge);

            var now = TimeSpan.FromHours(1);
            var options = new CookieBuilder { MaxAge = now }.Build(new DefaultProtoContext());
            Assert.Equal(now, options.MaxAge);
        }

        [Fact]
        public void CookieBuilderPreservesDefaultPath()
        {
            Assert.Equal(new CookieOptions().Path, new CookieBuilder().Build(new DefaultProtoContext()).Path);
        }
    }
}
