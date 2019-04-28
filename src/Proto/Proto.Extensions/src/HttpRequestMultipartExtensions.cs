// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Net.Proto.Headers;

namespace Contoso.GameNetCore.Proto.Extensions
{
    public static class ProtoRequestMultipartExtensions
    {
        public static string GetMultipartBoundary(this ProtoRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            MediaTypeHeaderValue mediaType;
            if (!MediaTypeHeaderValue.TryParse(request.ContentType, out mediaType))
            {
                return string.Empty;
            }
            return HeaderUtilities.RemoveQuotes(mediaType.Boundary).ToString();
        }
    }
}
