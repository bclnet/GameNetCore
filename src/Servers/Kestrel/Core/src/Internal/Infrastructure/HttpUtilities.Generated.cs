// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using Contoso.GameNetCore.Proto;
using Contoso.GameNetCore.Server.Kestrel.Core.Internal.Proto;

namespace Contoso.GameNetCore.Server.Kestrel.Core.Internal.Infrastructure
{
    public static partial class ProtoUtilities
    {
        // readonly primitive statics can be Jit'd to consts https://github.com/dotnet/coreclr/issues/1079
        private static readonly ulong _httpConnectMethodLong = GetAsciiStringAsLong("CONNECT ");
        private static readonly ulong _httpDeleteMethodLong = GetAsciiStringAsLong("DELETE \0");
        private static readonly ulong _httpHeadMethodLong = GetAsciiStringAsLong("HEAD \0\0\0");
        private static readonly ulong _httpPatchMethodLong = GetAsciiStringAsLong("PATCH \0\0");
        private static readonly ulong _httpPostMethodLong = GetAsciiStringAsLong("POST \0\0\0");
        private static readonly ulong _httpPutMethodLong = GetAsciiStringAsLong("PUT \0\0\0\0");
        private static readonly ulong _httpOptionsMethodLong = GetAsciiStringAsLong("OPTIONS ");
        private static readonly ulong _httpTraceMethodLong = GetAsciiStringAsLong("TRACE \0\0");

        private static readonly ulong _mask8Chars = GetMaskAsLong(new byte[]
            {0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff});

        private static readonly ulong _mask7Chars = GetMaskAsLong(new byte[]
            {0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00});

        private static readonly ulong _mask6Chars = GetMaskAsLong(new byte[]
            {0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00, 0x00});

        private static readonly ulong _mask5Chars = GetMaskAsLong(new byte[]
            {0xff, 0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00});

        private static readonly ulong _mask4Chars = GetMaskAsLong(new byte[]
            {0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00});

        private static readonly Tuple<ulong, ulong, ProtoMethod, int>[] _knownMethods =
            new Tuple<ulong, ulong, ProtoMethod, int>[17];

        private static readonly string[] _methodNames = new string[9];

        static ProtoUtilities()
        {
            SetKnownMethod(_mask4Chars, _httpPutMethodLong, ProtoMethod.Put, 3);
            SetKnownMethod(_mask5Chars, _httpHeadMethodLong, ProtoMethod.Head, 4);
            SetKnownMethod(_mask5Chars, _httpPostMethodLong, ProtoMethod.Post, 4);
            SetKnownMethod(_mask6Chars, _httpPatchMethodLong, ProtoMethod.Patch, 5);
            SetKnownMethod(_mask6Chars, _httpTraceMethodLong, ProtoMethod.Trace, 5);
            SetKnownMethod(_mask7Chars, _httpDeleteMethodLong, ProtoMethod.Delete, 6);
            SetKnownMethod(_mask8Chars, _httpConnectMethodLong, ProtoMethod.Connect, 7);
            SetKnownMethod(_mask8Chars, _httpOptionsMethodLong, ProtoMethod.Options, 7);
            FillKnownMethodsGaps();
            _methodNames[(byte)ProtoMethod.Connect] = ProtoMethods.Connect;
            _methodNames[(byte)ProtoMethod.Delete] = ProtoMethods.Delete;
            _methodNames[(byte)ProtoMethod.Get] = ProtoMethods.Get;
            _methodNames[(byte)ProtoMethod.Head] = ProtoMethods.Head;
            _methodNames[(byte)ProtoMethod.Options] = ProtoMethods.Options;
            _methodNames[(byte)ProtoMethod.Patch] = ProtoMethods.Patch;
            _methodNames[(byte)ProtoMethod.Post] = ProtoMethods.Post;
            _methodNames[(byte)ProtoMethod.Put] = ProtoMethods.Put;
            _methodNames[(byte)ProtoMethod.Trace] = ProtoMethods.Trace;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetKnownMethodIndex(ulong value)
        {
            const int magicNumer = 0x600000C;
            var tmp = (int)value & magicNumer;
            return ((tmp >> 2) | (tmp >> 23)) & 0xF;
        }
    }
}