// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Contoso.GameNetCore.Builder;
using Contoso.GameNetCore.Proto;
using Microsoft.Extensions.Options;

namespace Contoso.GameNetCore.ProtoOverrides
{
    public class ProtoMethodOverrideMiddleware
    {
        private const string xProtoMethodOverride = "X-Proto-Method-Override";
        private readonly RequestDelegate _next;
        private readonly ProtoMethodOverrideOptions _options;

        public ProtoMethodOverrideMiddleware(RequestDelegate next, IOptions<ProtoMethodOverrideOptions> options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _next = next;
            _options = options.Value;
        }

        public async Task Invoke(ProtoContext context)
        {
            if (string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
            {
                if (_options.FormFieldName != null)
                {
                    if (context.Request.HasFormContentType)
                    {
                        var form = await context.Request.ReadFormAsync();
                        var methodType = form[_options.FormFieldName];
                        if (!string.IsNullOrEmpty(methodType))
                        {
                            context.Request.Method = methodType;
                        }
                    }
                }
                else
                {
                    var xProtoMethodOverrideValue = context.Request.Headers[xProtoMethodOverride];
                    if (!string.IsNullOrEmpty(xProtoMethodOverrideValue))
                    {
                        context.Request.Method = xProtoMethodOverrideValue;
                    }
                }
            }
            await _next(context);
        }
    }
}
