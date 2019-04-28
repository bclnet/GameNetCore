// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Contoso.GameNetCore.Builder
{
    public class ProtoMethodOverrideOptions
    {
        /// <summary>
        /// Denotes the form element that contains the name of the resulting method type.
        /// If not set the X-Proto-Method-Override header will be used.
        /// </summary>
        public string FormFieldName { get; set; }
    }
}
