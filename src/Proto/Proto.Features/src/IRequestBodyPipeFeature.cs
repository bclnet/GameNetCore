// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO.Pipelines;

namespace Contoso.GameNetCore.Proto.Features
{
    /// <summary>
    /// Represents the ProtoRequestBody as a PipeReader.
    /// </summary>
    public interface IRequestBodyPipeFeature
    {
        /// <summary>
        /// A <see cref="PipeReader"/> representing the request body, if any.
        /// </summary>
        PipeReader Reader { get; set; }
    }
}
