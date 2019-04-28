// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Contoso.GameNetCore.Builder.Internal;
using Contoso.GameNetCore.Proto;
using Xunit;

namespace Contoso.GameNetCore.Builder.Extensions
{
    using Predicate = Func<ProtoContext, bool>;

    public class MapPredicateMiddlewareTests
    {
        private static readonly Predicate NotImplementedPredicate = new Predicate(environment => { throw new NotImplementedException(); });

        private static Task Success(ProtoContext context)
        {
            context.Response.StatusCode = 200;
            return Task.FromResult<object>(null);
        }

        private static void UseSuccess(IApplicationBuilder app)
        {
            app.Run(Success);
        }

        private static Task NotImplemented(ProtoContext context)
        {
            throw new NotImplementedException();
        }

        private static void UseNotImplemented(IApplicationBuilder app)
        {
            app.Run(NotImplemented);
        }

        private bool TruePredicate(ProtoContext context)
        {
            return true;
        }

        private bool FalsePredicate(ProtoContext context)
        {
            return false;
        }

        [Fact]
        public void NullArguments_ArgumentNullException()
        {
            var builder = new ApplicationBuilder(serviceProvider: null);
            var noMiddleware = new ApplicationBuilder(serviceProvider: null).Build();
            var noOptions = new MapWhenOptions();
            Assert.Throws<ArgumentNullException>(() => builder.MapWhen(null, UseNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapWhen(NotImplementedPredicate, configuration: null));
            Assert.Throws<ArgumentNullException>(() => new MapWhenMiddleware(null, noOptions));
            Assert.Throws<ArgumentNullException>(() => new MapWhenMiddleware(noMiddleware, null));
            Assert.Throws<ArgumentNullException>(() => new MapWhenMiddleware(null, noOptions));
            Assert.Throws<ArgumentNullException>(() => new MapWhenMiddleware(noMiddleware, null));
        }

        [Fact]
        public void PredicateTrue_BranchTaken()
        {
            ProtoContext context = CreateRequest();
            var builder = new ApplicationBuilder(serviceProvider: null);
            builder.MapWhen(TruePredicate, UseSuccess);
            var app = builder.Build();
            app.Invoke(context).Wait();

            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public void PredicateTrueAction_BranchTaken()
        {
            ProtoContext context = CreateRequest();
            var builder = new ApplicationBuilder(serviceProvider: null);
            builder.MapWhen(TruePredicate, UseSuccess);
            var app = builder.Build();
            app.Invoke(context).Wait();

            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public void PredicateFalseAction_PassThrough()
        {
            ProtoContext context = CreateRequest();
            var builder = new ApplicationBuilder(serviceProvider: null);
            builder.MapWhen(FalsePredicate, UseNotImplemented);
            builder.Run(Success);
            var app = builder.Build();
            app.Invoke(context).Wait();

            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public void ChainedPredicates_Success()
        {
            var builder = new ApplicationBuilder(serviceProvider: null);
            builder.MapWhen(TruePredicate, map1 =>
            {
                map1.MapWhen((Predicate)FalsePredicate, UseNotImplemented);
                map1.MapWhen((Predicate)TruePredicate, map2 => map2.MapWhen((Predicate)TruePredicate, UseSuccess));
                map1.Run(NotImplemented);
            });
            var app = builder.Build();

            ProtoContext context = CreateRequest();
            app.Invoke(context).Wait();
            Assert.Equal(200, context.Response.StatusCode);
        }

        private ProtoContext CreateRequest()
        {
            ProtoContext context = new DefaultProtoContext();
            return context;
        }
    }
}
