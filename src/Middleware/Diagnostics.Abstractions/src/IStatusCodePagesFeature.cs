// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Diagnostics
{
    /// <summary>
    /// Represents the Status code pages feature.
    /// </summary>
    public interface IStatusCodePagesFeature
    {
        /// <summary>
        /// Indicates if the status code middleware will handle responses.
        /// </summary>
        bool Enabled { get; set; }
    }
}