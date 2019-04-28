// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.GameSockets;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Contoso.GameNetCore.Proto.Features;
using Xunit;

namespace Contoso.GameNetCore.Proto
{
    public class ProtoContextAccessorTests
    {
        [Fact]
        public async Task ProtoContextAccessor_GettingProtoContextReturnsProtoContext()
        {
            var accessor = new ProtoContextAccessor();

            var context = new DefaultProtoContext();
            context.TraceIdentifier = "1";
            accessor.ProtoContext = context;

            await Task.Delay(100);

            Assert.Same(context, accessor.ProtoContext);
        }

        [Fact]
        public void ProtoContextAccessor_GettingProtoContextWithOutSettingReturnsNull()
        {
            var accessor = new ProtoContextAccessor();

            Assert.Null(accessor.ProtoContext);
        }

        [Fact]
        public async Task ProtoContextAccessor_GettingProtoContextReturnsNullProtoContextIfSetToNull()
        {
            var accessor = new ProtoContextAccessor();

            var context = new DefaultProtoContext();
            accessor.ProtoContext = context;

            var checkAsyncFlowTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var waitForNullTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var afterNullCheckTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            ThreadPool.QueueUserWorkItem(async _ =>
            {
                // The ProtoContext flows with the execution context
                Assert.Same(context, accessor.ProtoContext);

                checkAsyncFlowTcs.SetResult(null);

                await waitForNullTcs.Task;

                try
                {
                    Assert.Null(accessor.ProtoContext);

                    afterNullCheckTcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    afterNullCheckTcs.SetException(ex);
                }
            });

            await checkAsyncFlowTcs.Task;

            // Null out the accessor
            accessor.ProtoContext = null;

            waitForNullTcs.SetResult(null);

            Assert.Null(accessor.ProtoContext);

            await afterNullCheckTcs.Task;
        }

        [Fact]
        public async Task ProtoContextAccessor_GettingProtoContextReturnsNullProtoContextIfChanged()
        {
            var accessor = new ProtoContextAccessor();

            var context = new DefaultProtoContext();
            accessor.ProtoContext = context;

            var checkAsyncFlowTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var waitForNullTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var afterNullCheckTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            ThreadPool.QueueUserWorkItem(async _ =>
            {
                // The ProtoContext flows with the execution context
                Assert.Same(context, accessor.ProtoContext);

                checkAsyncFlowTcs.SetResult(null);

                await waitForNullTcs.Task;

                try
                {
                    Assert.Null(accessor.ProtoContext);

                    afterNullCheckTcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    afterNullCheckTcs.SetException(ex);
                }
            });

            await checkAsyncFlowTcs.Task;

            // Set a new http context
            var context2 = new DefaultProtoContext();
            accessor.ProtoContext = context2;

            waitForNullTcs.SetResult(null);

            Assert.Same(context2, accessor.ProtoContext);

            await afterNullCheckTcs.Task;
        }

        [Fact]
        public async Task ProtoContextAccessor_GettingProtoContextDoesNotFlowIfAccessorSetToNull()
        {
            var accessor = new ProtoContextAccessor();

            var context = new DefaultProtoContext();
            accessor.ProtoContext = context;

            var checkAsyncFlowTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            accessor.ProtoContext = null;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    // The ProtoContext flows with the execution context
                    Assert.Null(accessor.ProtoContext);
                    checkAsyncFlowTcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    checkAsyncFlowTcs.SetException(ex);
                }
            });

            await checkAsyncFlowTcs.Task;
        }

        [Fact]
        public async Task ProtoContextAccessor_GettingProtoContextDoesNotFlowIfExecutionContextDoesNotFlow()
        {
            var accessor = new ProtoContextAccessor();

            var context = new DefaultProtoContext();
            accessor.ProtoContext = context;

            var checkAsyncFlowTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                try
                {
                    // The ProtoContext flows with the execution context
                    Assert.Null(accessor.ProtoContext);
                    checkAsyncFlowTcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    checkAsyncFlowTcs.SetException(ex);
                }
            }, null);

            await checkAsyncFlowTcs.Task;
        }
    }
}