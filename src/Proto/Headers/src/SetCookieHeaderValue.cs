// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Proto.Headers
{
    // http://tools.ietf.org/html/rfc6265
    public class SetCookieHeaderValue
    {
        private const string ExpiresToken = "expires";
        private const string MaxAgeToken = "max-age";
        private const string DomainToken = "domain";
        private const string PathToken = "path";
        private const string SecureToken = "secure";
        // RFC Draft: https://tools.ietf.org/html/draft-ietf-httpbis-cookie-same-site-00
        private const string SameSiteToken = "samesite";
        private static readonly string SameSiteLaxToken = SameSiteMode.Lax.ToString().ToLower();
        private static readonly string SameSiteStrictToken = SameSiteMode.Strict.ToString().ToLower();
        private const string ProtoOnlyToken = "httponly";
        private const string SeparatorToken = "; ";
        private const string EqualsToken = "=";
        private const int ExpiresDateLength = 29;
        private const string ExpiresDateFormat = "r";

        private static readonly ProtoHeaderParser<SetCookieHeaderValue> SingleValueParser
            = new GenericHeaderParser<SetCookieHeaderValue>(false, GetSetCookieLength);
        private static readonly ProtoHeaderParser<SetCookieHeaderValue> MultipleValueParser
            = new GenericHeaderParser<SetCookieHeaderValue>(true, GetSetCookieLength);

        private StringSegment _name;
        private StringSegment _value;

        private SetCookieHeaderValue()
        {
            // Used by the parser to create a new instance of this type.
        }

        public SetCookieHeaderValue(StringSegment name)
            : this(name, StringSegment.Empty)
        {
        }

        public SetCookieHeaderValue(StringSegment name, StringSegment value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Name = name;
            Value = value;
        }

        public StringSegment Name
        {
            get { return _name; }
            set
            {
                CookieHeaderValue.CheckNameFormat(value, nameof(value));
                _name = value;
            }
        }

        public StringSegment Value
        {
            get { return _value; }
            set
            {
                CookieHeaderValue.CheckValueFormat(value, nameof(value));
                _value = value;
            }
        }

        public DateTimeOffset? Expires { get; set; }

        public TimeSpan? MaxAge { get; set; }

        public StringSegment Domain { get; set; }

        public StringSegment Path { get; set; }

        public bool Secure { get; set; }

        public SameSiteMode SameSite { get; set; }

        public bool ProtoOnly { get; set; }

        // name="value"; expires=Sun, 06 Nov 1994 08:49:37 GMT; max-age=86400; domain=domain1; path=path1; secure; samesite={Strict|Lax}; httponly
        public override string ToString()
        {
            var length = _name.Length + EqualsToken.Length + _value.Length;

            string maxAge = null;

            if (Expires.HasValue)
            {
                length += SeparatorToken.Length + ExpiresToken.Length + EqualsToken.Length + ExpiresDateLength;
            }

            if (MaxAge.HasValue)
            {
                maxAge = HeaderUtilities.FormatNonNegativeInt64((long)MaxAge.GetValueOrDefault().TotalSeconds);
                length += SeparatorToken.Length + MaxAgeToken.Length + EqualsToken.Length + maxAge.Length;
            }

            if (Domain != null)
            {
                length += SeparatorToken.Length + DomainToken.Length + EqualsToken.Length + Domain.Length;
            }

            if (Path != null)
            {
                length += SeparatorToken.Length + PathToken.Length + EqualsToken.Length + Path.Length;
            }

            if (Secure)
            {
                length += SeparatorToken.Length + SecureToken.Length;
            }

            if (SameSite != SameSiteMode.None)
            {
                var sameSite = SameSite == SameSiteMode.Lax ? SameSiteLaxToken : SameSiteStrictToken;
                length += SeparatorToken.Length + SameSiteToken.Length + EqualsToken.Length + sameSite.Length;
            }

            if (ProtoOnly)
            {
                length += SeparatorToken.Length + ProtoOnlyToken.Length;
            }

            return string.Create(length, (this, maxAge), (span, tuple) =>
            {
                var (headerValue, maxAgeValue) = tuple;

                Append(ref span, headerValue._name);
                Append(ref span, EqualsToken);
                Append(ref span, headerValue._value);

                if (headerValue.Expires is DateTimeOffset expiresValue)
                {
                    Append(ref span, SeparatorToken);
                    Append(ref span, ExpiresToken);
                    Append(ref span, EqualsToken);

                    var formatted = expiresValue.TryFormat(span, out var charsWritten, ExpiresDateFormat);
                    span = span.Slice(charsWritten);

                    Debug.Assert(formatted);
                }

                if (maxAgeValue != null)
                {
                    AppendSegment(ref span, MaxAgeToken, maxAgeValue);
                }

                if (headerValue.Domain != null)
                {
                    AppendSegment(ref span, DomainToken, headerValue.Domain);
                }

                if (headerValue.Path != null)
                {
                    AppendSegment(ref span, PathToken, headerValue.Path);
                }

                if (headerValue.Secure)
                {
                    AppendSegment(ref span, SecureToken, null);
                }

                if (headerValue.SameSite != SameSiteMode.None)
                {
                    AppendSegment(ref span, SameSiteToken, headerValue.SameSite == SameSiteMode.Lax ? SameSiteLaxToken : SameSiteStrictToken);
                }

                if (headerValue.ProtoOnly)
                {
                    AppendSegment(ref span, ProtoOnlyToken, null);
                }
            });
        }

        private static void AppendSegment(ref Span<char> span, StringSegment name, StringSegment value)
        {
            Append(ref span, SeparatorToken);
            Append(ref span, name.AsSpan());
            if (value != null)
            {
                Append(ref span, EqualsToken);
                Append(ref span, value.AsSpan());
            }
        }

        private static void Append(ref Span<char> span, ReadOnlySpan<char> other)
        {
            other.CopyTo(span);
            span = span.Slice(other.Length);
        }

        /// <summary>
        /// Append string representation of this <see cref="SetCookieHeaderValue"/> to given
        /// <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="StringBuilder"/> to receive the string representation of this
        /// <see cref="SetCookieHeaderValue"/>.
        /// </param>
        public void AppendToStringBuilder(StringBuilder builder)
        {
            builder.Append(_name.AsSpan());
            builder.Append("=");
            builder.Append(_value.AsSpan());

            if (Expires.HasValue)
            {
                AppendSegment(builder, ExpiresToken, HeaderUtilities.FormatDate(Expires.GetValueOrDefault()));
            }

            if (MaxAge.HasValue)
            {
                AppendSegment(builder, MaxAgeToken, HeaderUtilities.FormatNonNegativeInt64((long)MaxAge.GetValueOrDefault().TotalSeconds));
            }

            if (Domain != null)
            {
                AppendSegment(builder, DomainToken, Domain);
            }

            if (Path != null)
            {
                AppendSegment(builder, PathToken, Path);
            }

            if (Secure)
            {
                AppendSegment(builder, SecureToken, null);
            }

            if (SameSite != SameSiteMode.None)
            {
                AppendSegment(builder, SameSiteToken, SameSite == SameSiteMode.Lax ? SameSiteLaxToken : SameSiteStrictToken);
            }

            if (ProtoOnly)
            {
                AppendSegment(builder, ProtoOnlyToken, null);
            }
        }

        private static void AppendSegment(StringBuilder builder, StringSegment name, StringSegment value)
        {
            builder.Append("; ");
            builder.Append(name.AsSpan());
            if (value != null)
            {
                builder.Append("=");
                builder.Append(value.AsSpan());
            }
        }

        public static SetCookieHeaderValue Parse(StringSegment input)
        {
            var index = 0;
            return SingleValueParser.ParseValue(input, ref index);
        }

        public static bool TryParse(StringSegment input, out SetCookieHeaderValue parsedValue)
        {
            var index = 0;
            return SingleValueParser.TryParseValue(input, ref index, out parsedValue);
        }

        public static IList<SetCookieHeaderValue> ParseList(IList<string> inputs)
        {
            return MultipleValueParser.ParseValues(inputs);
        }

        public static IList<SetCookieHeaderValue> ParseStrictList(IList<string> inputs)
        {
            return MultipleValueParser.ParseStrictValues(inputs);
        }

        public static bool TryParseList(IList<string> inputs, out IList<SetCookieHeaderValue> parsedValues)
        {
            return MultipleValueParser.TryParseValues(inputs, out parsedValues);
        }

        public static bool TryParseStrictList(IList<string> inputs, out IList<SetCookieHeaderValue> parsedValues)
        {
            return MultipleValueParser.TryParseStrictValues(inputs, out parsedValues);
        }

        // name=value; expires=Sun, 06 Nov 1994 08:49:37 GMT; max-age=86400; domain=domain1; path=path1; secure; samesite={Strict|Lax}; httponly
        private static int GetSetCookieLength(StringSegment input, int startIndex, out SetCookieHeaderValue parsedValue)
        {
            Contract.Requires(startIndex >= 0);
            var offset = startIndex;

            parsedValue = null;

            if (StringSegment.IsNullOrEmpty(input) || (offset >= input.Length))
            {
                return 0;
            }

            var result = new SetCookieHeaderValue();

            // The caller should have already consumed any leading whitespace, commas, etc..

            // Name=value;

            // Name
            var itemLength = ProtoRuleParser.GetTokenLength(input, offset);
            if (itemLength == 0)
            {
                return 0;
            }
            result._name = input.Subsegment(offset, itemLength);
            offset += itemLength;

            // = (no spaces)
            if (!ReadEqualsSign(input, ref offset))
            {
                return 0;
            }

            // value or "quoted value"
            // The value may be empty
            result._value = CookieHeaderValue.GetCookieValue(input, ref offset);

            // *(';' SP cookie-av)
            while (offset < input.Length)
            {
                if (input[offset] == ',')
                {
                    // Divider between headers
                    break;
                }
                if (input[offset] != ';')
                {
                    // Expecting a ';' between parameters
                    return 0;
                }
                offset++;

                offset += ProtoRuleParser.GetWhitespaceLength(input, offset);

                //  cookie-av = expires-av / max-age-av / domain-av / path-av / secure-av / samesite-av / httponly-av / extension-av
                itemLength = ProtoRuleParser.GetTokenLength(input, offset);
                if (itemLength == 0)
                {
                    // Trailing ';' or leading into garbage. Let the next parser fail.
                    break;
                }
                var token = input.Subsegment(offset, itemLength);
                offset += itemLength;

                //  expires-av = "Expires=" sane-cookie-date
                if (StringSegment.Equals(token, ExpiresToken, StringComparison.OrdinalIgnoreCase))
                {
                    // = (no spaces)
                    if (!ReadEqualsSign(input, ref offset))
                    {
                        return 0;
                    }
                    var dateString = ReadToSemicolonOrEnd(input, ref offset);
                    DateTimeOffset expirationDate;
                    if (!ProtoRuleParser.TryStringToDate(dateString, out expirationDate))
                    {
                        // Invalid expiration date, abort
                        return 0;
                    }
                    result.Expires = expirationDate;
                }
                // max-age-av = "Max-Age=" non-zero-digit *DIGIT
                else if (StringSegment.Equals(token, MaxAgeToken, StringComparison.OrdinalIgnoreCase))
                {
                    // = (no spaces)
                    if (!ReadEqualsSign(input, ref offset))
                    {
                        return 0;
                    }

                    itemLength = ProtoRuleParser.GetNumberLength(input, offset, allowDecimal: false);
                    if (itemLength == 0)
                    {
                        return 0;
                    }
                    var numberString = input.Subsegment(offset, itemLength);
                    long maxAge;
                    if (!HeaderUtilities.TryParseNonNegativeInt64(numberString, out maxAge))
                    {
                        // Invalid expiration date, abort
                        return 0;
                    }
                    result.MaxAge = TimeSpan.FromSeconds(maxAge);
                    offset += itemLength;
                }
                // domain-av = "Domain=" domain-value
                // domain-value = <subdomain> ; defined in [RFC1034], Section 3.5, as enhanced by [RFC1123], Section 2.1
                else if (StringSegment.Equals(token, DomainToken, StringComparison.OrdinalIgnoreCase))
                {
                    // = (no spaces)
                    if (!ReadEqualsSign(input, ref offset))
                    {
                        return 0;
                    }
                    // We don't do any detailed validation on the domain.
                    result.Domain = ReadToSemicolonOrEnd(input, ref offset);
                }
                // path-av = "Path=" path-value
                // path-value = <any CHAR except CTLs or ";">
                else if (StringSegment.Equals(token, PathToken, StringComparison.OrdinalIgnoreCase))
                {
                    // = (no spaces)
                    if (!ReadEqualsSign(input, ref offset))
                    {
                        return 0;
                    }
                    // We don't do any detailed validation on the path.
                    result.Path = ReadToSemicolonOrEnd(input, ref offset);
                }
                // secure-av = "Secure"
                else if (StringSegment.Equals(token, SecureToken, StringComparison.OrdinalIgnoreCase))
                {
                    result.Secure = true;
                }
                // samesite-av = "SameSite" / "SameSite=" samesite-value
                // samesite-value = "Strict" / "Lax"
                else if (StringSegment.Equals(token, SameSiteToken, StringComparison.OrdinalIgnoreCase))
                {
                    if (!ReadEqualsSign(input, ref offset))
                    {
                        result.SameSite = SameSiteMode.Strict;
                    }
                    else
                    {
                        var enforcementMode = ReadToSemicolonOrEnd(input, ref offset);

                        if (StringSegment.Equals(enforcementMode, SameSiteLaxToken, StringComparison.OrdinalIgnoreCase))
                        {
                            result.SameSite = SameSiteMode.Lax;
                        }
                        else
                        {
                            result.SameSite = SameSiteMode.Strict;
                        }
                    }
                }
                // httponly-av = "ProtoOnly"
                else if (StringSegment.Equals(token, ProtoOnlyToken, StringComparison.OrdinalIgnoreCase))
                {
                    result.ProtoOnly = true;
                }
                // extension-av = <any CHAR except CTLs or ";">
                else
                {
                    // TODO: skiping it for now to avoid parsing failure? Store it in a list?
                    // = (no spaces)
                    if (!ReadEqualsSign(input, ref offset))
                    {
                        return 0;
                    }
                    ReadToSemicolonOrEnd(input, ref offset);
                }
            }

            parsedValue = result;
            return offset - startIndex;
        }

        private static bool ReadEqualsSign(StringSegment input, ref int offset)
        {
            // = (no spaces)
            if (offset >= input.Length || input[offset] != '=')
            {
                return false;
            }
            offset++;
            return true;
        }

        private static StringSegment ReadToSemicolonOrEnd(StringSegment input, ref int offset)
        {
            var end = input.IndexOf(';', offset);
            if (end < 0)
            {
                // Remainder of the string
                end = input.Length;
            }
            var itemLength = end - offset;
            var result = input.Subsegment(offset, itemLength);
            offset += itemLength;
            return result;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SetCookieHeaderValue;

            if (other == null)
            {
                return false;
            }

            return StringSegment.Equals(_name, other._name, StringComparison.OrdinalIgnoreCase)
                && StringSegment.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase)
                && Expires.Equals(other.Expires)
                && MaxAge.Equals(other.MaxAge)
                && StringSegment.Equals(Domain, other.Domain, StringComparison.OrdinalIgnoreCase)
                && StringSegment.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase)
                && Secure == other.Secure
                && SameSite == other.SameSite
                && ProtoOnly == other.ProtoOnly;
        }

        public override int GetHashCode()
        {
            return StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(_name)
                ^ StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(_value)
                ^ (Expires.HasValue ? Expires.GetHashCode() : 0)
                ^ (MaxAge.HasValue ? MaxAge.GetHashCode() : 0)
                ^ (Domain != null ? StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(Domain) : 0)
                ^ (Path != null ? StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(Path) : 0)
                ^ Secure.GetHashCode()
                ^ SameSite.GetHashCode()
                ^ ProtoOnly.GetHashCode();
        }
    }
}
