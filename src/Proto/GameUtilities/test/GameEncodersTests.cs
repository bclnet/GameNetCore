// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Contoso.GameNetCore.GameUtilities
{
    public class GameEncodersTests
    {

        [Theory]
        [InlineData("", 1, 0)]
        [InlineData("", 0, 1)]
        [InlineData("0123456789", 9, 2)]
        [InlineData("0123456789", Int32.MaxValue, 2)]
        [InlineData("0123456789", 9, -1)]
        public void Base64UrlDecode_BadOffsets(string input, int offset, int count)
        {
            // Act & assert
            Assert.ThrowsAny<ArgumentException>(() =>
            {
                var retVal = GameEncoders.Base64UrlDecode(input, offset, count);
            });
        }

        [Theory]
        [InlineData(0, 1, 0)]
        [InlineData(0, 0, 1)]
        [InlineData(10, 9, 2)]
        [InlineData(10, Int32.MaxValue, 2)]
        [InlineData(10, 9, -1)]
        public void Base64UrlEncode_BadOffsets(int inputLength, int offset, int count)
        {
            // Arrange
            var input = new byte[inputLength];

            // Act & assert
            Assert.ThrowsAny<ArgumentException>(() =>
            {
                var retVal = GameEncoders.Base64UrlEncode(input, offset, count);
            });
        }

        [Fact]
        public void DataOfVariousLengthRoundTripCorrectly()
        {
            for (var length = 0; length != 256; ++length)
            {
                var data = new byte[length];
                for (var index = 0; index != length; ++index)
                    data[index] = (byte)(5 + length + (index * 23));
                var text = GameEncoders.Base64UrlEncode(data);
                var result = GameEncoders.Base64UrlDecode(text);
                for (var index = 0; index != length; ++index)
                    Assert.Equal(data[index], result[index]);
            }
        }
    }
}
