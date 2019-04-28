// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.HostFiltering;
using Contoso.GameNetCore.Hosting;
using Contoso.GameNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HostFilteringSample
{
    public class Startup
    {
        public IConfiguration Config { get; }

        public Startup(IConfiguration config)
        {
            Config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostFiltering(options =>
            {

            });

            // Fallback
            services.PostConfigure<HostFilteringOptions>(options =>
            {
                if (options.AllowedHosts == null || options.AllowedHosts.Count == 0)
                {
                    // "AllowedHosts": "localhost;127.0.0.1;[::1]"
                    var hosts = Config["AllowedHosts"]?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    // Fall back to "*" to disable.
                    options.AllowedHosts = (hosts?.Length > 0 ? hosts : new[] { "*" });
                }
            });
            // Change notification
            services.AddSingleton<IOptionsChangeTokenSource<HostFilteringOptions>>(new ConfigurationChangeTokenSource<HostFilteringOptions>(Config));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseHostFiltering();

            app.Run(context =>
            {
                return context.Response.WriteAsync("Hello World! " + context.Request.Host);
            });
        }
    }
}
