// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Proto.Headers
{
    internal abstract class BaseHeaderParser<T> : ProtoHeaderParser<T>
    {
        protected BaseHeaderParser(bool supportsMultipleValues)
            : base(supportsMultipleValues) { }

        protected abstract int GetParsedValueLength(StringSegment value, int startIndex, out T parsedValue);

        public sealed override bool TryParseValue(StringSegment value, ref int index, out T parsedValue)
        {
            parsedValue = default(T);

            // If multiple values are supported (i.e. list of values), then accept an empty string: The header may
            // be added multiple times to the request/response message. E.g.
            //  Accept: text/xml; q=1
            //  Accept:
            //  Accept: text/plain; q=0.2
            if (StringSegment.IsNullOrEmpty(value) || (index == value.Length))
                return SupportsMultipleValues;

            var current = HeaderUtilities.GetNextNonEmptyOrWhitespaceIndex(value, index, SupportsMultipleValues,
                out var separatorFound);

            if (separatorFound && !SupportsMultipleValues)
                return false; // leading separators not allowed if we don't support multiple values.

            if (current == value.Length)
            {
                if (SupportsMultipleValues)
                    index = current;
                return SupportsMultipleValues;
            }

            var length = GetParsedValueLength(value, current, out var result);

            if (length == 0)
                return false;

            current += length;
            current = HeaderUtilities.GetNextNonEmptyOrWhitespaceIndex(value, current, SupportsMultipleValues,
                out separatorFound);

            // If we support multiple values and we've not reached the end of the string, then we must have a separator.
            if ((separatorFound && !SupportsMultipleValues) || (!separatorFound && (current < value.Length)))
                return false;

            index = current;
            parsedValue = result;
            return true;
        }
    }
}
