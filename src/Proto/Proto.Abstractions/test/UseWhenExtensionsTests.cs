// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Contoso.GameNetCore.Builder.Internal;
using Contoso.GameNetCore.Proto;
using Xunit;

namespace Contoso.GameNetCore.Builder.Extensions
{
    public class UseWhenExtensionsTests
    {
        [Fact]
        public void NullArguments_ArgumentNullException()
        {
            // Arrange
            var builder = CreateBuilder();

            // Act
            Action nullPredicate = () => builder.UseWhen(null, app => { });
            Action nullConfiguration = () => builder.UseWhen(TruePredicate, null);

            // Assert
            Assert.Throws<ArgumentNullException>(nullPredicate);
            Assert.Throws<ArgumentNullException>(nullConfiguration);
        }

        [Fact]
        public void PredicateTrue_BranchTaken_WillRejoin()
        {
            // Arrange
            var context = CreateContext();
            var parent = CreateBuilder();

            parent.UseWhen(TruePredicate, child =>
            {
                child.UseWhen(TruePredicate, grandchild =>
                {
                    grandchild.Use(Increment("grandchild"));
                });

                child.Use(Increment("child"));
            });

            parent.Use(Increment("parent"));

            // Act
            parent.Build().Invoke(context).Wait();

            // Assert
            Assert.Equal(1, Count(context, "parent"));
            Assert.Equal(1, Count(context, "child"));
            Assert.Equal(1, Count(context, "grandchild"));
        }

        [Fact]
        public void PredicateTrue_BranchTaken_CanTerminate()
        {
            // Arrange
            var context = CreateContext();
            var parent = CreateBuilder();

            parent.UseWhen(TruePredicate, child =>
            {
                child.UseWhen(TruePredicate, grandchild =>
                {
                    grandchild.Use(Increment("grandchild", terminate: true));
                });

                child.Use(Increment("child"));
            });

            parent.Use(Increment("parent"));

            // Act
            parent.Build().Invoke(context).Wait();

            // Assert
            Assert.Equal(0, Count(context, "parent"));
            Assert.Equal(0, Count(context, "child"));
            Assert.Equal(1, Count(context, "grandchild"));
        }

        [Fact]
        public void PredicateFalse_PassThrough()
        {
            // Arrange
            var context = CreateContext();
            var parent = CreateBuilder();

            parent.UseWhen(FalsePredicate, child =>
            {
                child.Use(Increment("child"));
            });

            parent.Use(Increment("parent"));

            // Act
            parent.Build().Invoke(context).Wait();

            // Assert
            Assert.Equal(1, Count(context, "parent"));
            Assert.Equal(0, Count(context, "child"));
        }

        private static ProtoContext CreateContext()
        {
            return new DefaultProtoContext();
        }

        private static ApplicationBuilder CreateBuilder()
        {
            return new ApplicationBuilder(serviceProvider: null);
        }

        private static bool TruePredicate(ProtoContext context)
        {
            return true;
        }

        private static bool FalsePredicate(ProtoContext context)
        {
            return false;
        }

        private static Func<ProtoContext, Func<Task>, Task> Increment(string key, bool terminate = false)
        {
            return (context, next) =>
            {
                if (!context.Items.ContainsKey(key))
                {
                    context.Items[key] = 1;
                }
                else
                {
                    var item = context.Items[key];

                    if (item is int)
                    {
                        context.Items[key] = 1 + (int)item;
                    }
                    else
                    {
                        context.Items[key] = 1;
                    }
                }

                return terminate ? Task.FromResult<object>(null) : next();
            };
        }

        private static int Count(ProtoContext context, string key)
        {
            if (!context.Items.ContainsKey(key))
            {
                return 0;
            }

            var item = context.Items[key];

            if (item is int)
            {
                return (int)item;
            }

            return 0;
        }
    }
}
