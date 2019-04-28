// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;

namespace Contoso.GameNetCore.GameUtilities
{
    public class MultipartSection
    {
        public string ContentType => Headers.TryGetValue("Content-Type", out var values) ? (string)values : null;

        public string ContentDisposition => Headers.TryGetValue("Content-Disposition", out var values) ? (string)values : null;

        public Dictionary<string, StringValues> Headers { get; set; }

        public Stream Body { get; set; }

        /// <summary>
        /// The position where the body starts in the total multipart body.
        /// This may not be available if the total multipart body is not seekable.
        /// </summary>
        public long? BaseStreamOffset { get; set; }
    }
}