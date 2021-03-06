// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contoso.GameNetCore.HostFiltering;
using Contoso.GameNetCore.Hosting;
using Contoso.GameNetCore.Routing;
using Contoso.GameNetCore.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Contoso.GameNetCore.Tests
{
    public class WebHostTests
    {
        [Fact]
        public void WebHostConfiguration_IncludesCommandLineArguments()
        {
            var builder = WebHost.CreateDefaultBuilder(new string[] { "--urls", "http://localhost:5001" });
            Assert.Equal("http://localhost:5001", builder.GetSetting(WebHostDefaults.ServerUrlsKey));
        }

        [Fact]
        public async Task WebHostConfiguration_HostFilterOptionsAreReloadable()
        {
            var host = WebHost.CreateDefaultBuilder()
                .Configure(app => { })
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.Add(new ReloadableMemorySource());
                }).Build();
            var config = host.Services.GetRequiredService<IConfiguration>();
            var monitor = host.Services.GetRequiredService<IOptionsMonitor<HostFilteringOptions>>();
            var options = monitor.CurrentValue;

            Assert.Contains("*", options.AllowedHosts);

            var changed = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            monitor.OnChange(newOptions =>
            {
                changed.SetResult(0);
            });

            config["AllowedHosts"] = "NewHost";

            await changed.Task.TimeoutAfter(TimeSpan.FromSeconds(10));
            options = monitor.CurrentValue;
            Assert.Contains("NewHost", options.AllowedHosts);
        }

        [Fact]
        public void CreateDefaultBuilder_RegistersRouting()
        {
            var host = WebHost.CreateDefaultBuilder()
                .Configure(_ => { })
                .Build();

            var linkGenerator = host.Services.GetService(typeof(LinkGenerator));
            Assert.NotNull(linkGenerator);
        }

        [Fact]
        public void CreateDefaultBuilder_RegistersEventSourceLogger()
        {
            var listener = new TestEventListener();
            var host = WebHost.CreateDefaultBuilder()
                .Configure(_ => { })
                .Build();

            var logger = host.Services.GetRequiredService<ILogger<WebHostTests>>();
            logger.LogInformation("Request starting");

            var events = listener.EventData.ToArray();
            Assert.Contains(events, args =>
                args.EventSource.Name == "Microsoft-Extensions-Logging" &&
                args.Payload.OfType<string>().Any(p => p.Contains("Request starting")));
        }

        private class TestEventListener : EventListener
        {
            private volatile bool _disposed;

            private ConcurrentQueue<EventWrittenEventArgs> _events = new ConcurrentQueue<EventWrittenEventArgs>();

            public IEnumerable<EventWrittenEventArgs> EventData => _events;

            protected override void OnEventSourceCreated(EventSource eventSource)
            {
                if (eventSource.Name == "Microsoft-Extensions-Logging")
                {
                    EnableEvents(eventSource, EventLevel.Informational);
                }
            }

            protected override void OnEventWritten(EventWrittenEventArgs eventData)
            {
                if (!_disposed)
                {
                    _events.Enqueue(eventData);
                }
            }

            public override void Dispose()
            {
                _disposed = true;
                base.Dispose();
            }
        }

        private class ReloadableMemorySource : IConfigurationSource
        {
            public IConfigurationProvider Build(IConfigurationBuilder builder)
            {
                return new ReloadableMemoryProvider();
            }
        }

        private class ReloadableMemoryProvider : ConfigurationProvider
        {
            public override void Set(string key, string value)
            {
                base.Set(key, value);
                OnReload();
            }
        }
    }
}
