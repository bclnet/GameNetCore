// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;

namespace Contoso.GameNetCore.GameUtilities
{
    internal class MultipartBoundary
    {
        private readonly int[] _skipTable = new int[256];
        private readonly string _boundary;
        private bool _expectLeadingCrlf;

        public MultipartBoundary(string boundary, bool expectLeadingCrlf = true)
        {
            _boundary = boundary ?? throw new ArgumentNullException(nameof(boundary));
            _expectLeadingCrlf = expectLeadingCrlf;
            Initialize(_boundary, _expectLeadingCrlf);
        }

        private void Initialize(string boundary, bool expectLeadingCrlf)
        {
            BoundaryBytes = expectLeadingCrlf ? Encoding.UTF8.GetBytes("\r\n--" + boundary) : Encoding.UTF8.GetBytes("--" + boundary);
            FinalBoundaryLength = BoundaryBytes.Length + 2; // Include the final '--' terminator.

            var length = BoundaryBytes.Length;
            for (var i = 0; i < _skipTable.Length; ++i)
                _skipTable[i] = length;
            for (var i = 0; i < length; ++i)
                _skipTable[BoundaryBytes[i]] = Math.Max(1, length - 1 - i);
        }

        public int GetSkipValue(byte input) => _skipTable[input];

        public bool ExpectLeadingCrlf
        {
            get => _expectLeadingCrlf;
            set
            {
                if (value != _expectLeadingCrlf)
                {
                    _expectLeadingCrlf = value;
                    Initialize(_boundary, _expectLeadingCrlf);
                }
            }
        }

        public byte[] BoundaryBytes { get; private set; }

        public int FinalBoundaryLength { get; private set; }
    }
}
