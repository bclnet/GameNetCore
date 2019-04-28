// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Moq;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Contoso.GameNetCore.GameUtilities.Test
{
    public class ProtoResponseStreamWriterTest
    {
        private const int DefaultCharacterChunkSize = ProtoResponseStreamWriter.DefaultBufferSize;

        [Fact]
        public async Task DoesNotWriteBOM()
        {
            // Arrange
            var memoryStream = new MemoryStream();
            var encodingWithBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            var writer = new ProtoResponseStreamWriter(memoryStream, encodingWithBOM);
            var expectedData = new byte[] { 97, 98, 99, 100 }; // without BOM

            // Act
            using (writer)
                await writer.WriteAsync("abcd");

            // Assert
            Assert.Equal(expectedData, memoryStream.ToArray());
        }

        [Fact]
        public async Task DoesNotFlush_UnderlyingStream_OnDisposingWriter()
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new ProtoResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            await writer.WriteAsync("Hello");
            writer.Dispose();

            // Assert
            Assert.Equal(0, stream.FlushCallCount);
            Assert.Equal(0, stream.FlushAsyncCallCount);
        }

        [Fact]
        public async Task DoesNotDispose_UnderlyingStream_OnDisposingWriter()
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new ProtoResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            await writer.WriteAsync("Hello world");
            writer.Dispose();

            // Assert
            Assert.Equal(0, stream.DisposeCallCount);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1050)]
        [InlineData(2048)]
        public async Task FlushesBuffer_OnClose(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new ProtoResponseStreamWriter(stream, Encoding.UTF8);
            await writer.WriteAsync(new string('a', byteLength));

            // Act
            writer.Dispose();

            // Assert
            Assert.Equal(0, stream.FlushCallCount);
            Assert.Equal(0, stream.FlushAsyncCallCount);
            Assert.Equal(byteLength, stream.Length);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1050)]
        [InlineData(2048)]
        public async Task FlushesBuffer_OnDispose(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new ProtoResponseStreamWriter(stream, Encoding.UTF8);
            await writer.WriteAsync(new string('a', byteLength));

            // Act
            writer.Dispose();

            // Assert
            Assert.Equal(0, stream.FlushCallCount);
            Assert.Equal(0, stream.FlushAsyncCallCount);
            Assert.Equal(byteLength, stream.Length);
        }

        [Fact]
        public void NoDataWritten_Flush_DoesNotFlushUnderlyingStream()
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new ProtoResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            writer.Flush();

            // Assert
            Assert.Equal(0, stream.FlushCallCount);
            Assert.Equal(0, stream.Length);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1050)]
        [InlineData(2048)]
        public void FlushesBuffer_ButNotStream_OnFlush(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new ProtoResponseStreamWriter(stream, Encoding.UTF8);
            writer.Write(new string('a', byteLength));

            var expectedWriteCount = Math.Ceiling((double)byteLength / ProtoResponseStreamWriter.DefaultBufferSize);

            // Act
            writer.Flush();

            // Assert
            Assert.Equal(0, stream.FlushCallCount);
            Assert.Equal(expectedWriteCount, stream.WriteCallCount);
            Assert.Equal(byteLength, stream.Length);
        }

        [Fact]
        public async Task NoDataWritten_FlushAsync_DoesNotFlushUnderlyingStream()
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new ProtoResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            await writer.FlushAsync();

            // Assert
            Assert.Equal(0, stream.FlushAsyncCallCount);
            Assert.Equal(0, stream.Length);
        }

        [Theory]
        [InlineData(ProtoResponseStreamWriter.DefaultBufferSize - 1)]
        [InlineData(ProtoResponseStreamWriter.DefaultBufferSize)]
        [InlineData(ProtoResponseStreamWriter.DefaultBufferSize + 1)]
        [InlineData(ProtoResponseStreamWriter.DefaultBufferSize * 2)]
        public async Task FlushesBuffer_ButNotStream_OnFlushAsync(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new ProtoResponseStreamWriter(stream, Encoding.UTF8);
            await writer.WriteAsync(new string('a', byteLength));

            var expectedWriteCount = Math.Ceiling((double)byteLength / ProtoResponseStreamWriter.DefaultBufferSize);

            // Act
            await writer.FlushAsync();

            // Assert
            Assert.Equal(0, stream.FlushAsyncCallCount);
            Assert.Equal(expectedWriteCount, stream.WriteAsyncCallCount);
            Assert.Equal(byteLength, stream.Length);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        public async Task FlushWriteThrows_DontFlushInDispose(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream() { ThrowOnWrite = true };
            var writer = new ProtoResponseStreamWriter(stream, Encoding.UTF8);

            await writer.WriteAsync(new string('a', byteLength));
            await Assert.ThrowsAsync<IOException>(() => writer.FlushAsync());

            // Act
            writer.Dispose();

            // Assert
            Assert.Equal(1, stream.WriteAsyncCallCount);
            Assert.Equal(0, stream.WriteCallCount);
            Assert.Equal(0, stream.FlushCallCount);
            Assert.Equal(0, stream.FlushAsyncCallCount);
            Assert.Equal(0, stream.Length);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1050)]
        [InlineData(2048)]
        public void WriteChar_WritesToStream(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new ProtoResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            using (writer)
                for (var i = 0; i < byteLength; i++)
                    writer.Write('a');

            // Assert
            Assert.Equal(byteLength, stream.Length);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1050)]
        [InlineData(2048)]
        public void WriteCharArray_WritesToStream(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new ProtoResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            using (writer)
                writer.Write((new string('a', byteLength)).ToCharArray());

            // Assert
            Assert.Equal(byteLength, stream.Length);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1050)]
        [InlineData(2048)]
        public async Task WriteCharAsync_WritesToStream(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new ProtoResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            using (writer)
                for (var i = 0; i < byteLength; i++)
                    await writer.WriteAsync('a');

            // Assert
            Assert.Equal(byteLength, stream.Length);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1050)]
        [InlineData(2048)]
        public async Task WriteCharArrayAsync_WritesToStream(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new ProtoResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            using (writer)
                await writer.WriteAsync((new string('a', byteLength)).ToCharArray());

            // Assert
            Assert.Equal(byteLength, stream.Length);
        }

        [Theory]
        [InlineData("你好世界", "utf-16")]
        [InlineData("హలో ప్రపంచ", "iso-8859-1")]
        [InlineData("வணக்கம் உலக", "utf-32")]
        public async Task WritesData_InExpectedEncoding(string data, string encodingName)
        {
            // Arrange
            var encoding = Encoding.GetEncoding(encodingName);
            var expectedBytes = encoding.GetBytes(data);
            var stream = new MemoryStream();
            var writer = new ProtoResponseStreamWriter(stream, encoding);

            // Act
            using (writer)
                await writer.WriteAsync(data);

            // Assert
            Assert.Equal(expectedBytes, stream.ToArray());
        }

        [Theory]
        [InlineData('ん', 1023, "utf-8")]
        [InlineData('ん', 1024, "utf-8")]
        [InlineData('ん', 1050, "utf-8")]
        [InlineData('你', 1023, "utf-16")]
        [InlineData('你', 1024, "utf-16")]
        [InlineData('你', 1050, "utf-16")]
        [InlineData('హ', 1023, "iso-8859-1")]
        [InlineData('హ', 1024, "iso-8859-1")]
        [InlineData('హ', 1050, "iso-8859-1")]
        [InlineData('வ', 1023, "utf-32")]
        [InlineData('வ', 1024, "utf-32")]
        [InlineData('வ', 1050, "utf-32")]
        public async Task WritesData_OfDifferentLength_InExpectedEncoding(
            char character,
            int charCount,
            string encodingName)
        {
            // Arrange
            var encoding = Encoding.GetEncoding(encodingName);
            var data = new string(character, charCount);
            var expectedBytes = encoding.GetBytes(data);
            var stream = new MemoryStream();
            var writer = new ProtoResponseStreamWriter(stream, encoding);

            // Act
            using (writer)
                await writer.WriteAsync(data);

            // Assert
            Assert.Equal(expectedBytes, stream.ToArray());
        }

        // None of the code in ProtoResponseStreamWriter differs significantly when using pooled buffers.
        //
        // This test effectively verifies that things are correctly constructed and disposed. Pooled buffers
        // throw on the finalizer thread if not disposed, so that's why it's complicated.
        [Fact]
        public void ProtoResponseStreamWriter_UsingPooledBuffers()
        {
            // Arrange
            var encoding = Encoding.UTF8;
            var stream = new MemoryStream();

            var expectedBytes = encoding.GetBytes("Hello, World!");

            using (var writer = new ProtoResponseStreamWriter(
                stream,
                encoding,
                1024,
                ArrayPool<byte>.Shared,
                ArrayPool<char>.Shared))
                // Act
                writer.Write("Hello, World!");

            // Assert
            Assert.Equal(expectedBytes, stream.ToArray());
        }

        [Theory]
        [InlineData(DefaultCharacterChunkSize)]
        [InlineData(DefaultCharacterChunkSize * 2)]
        [InlineData(DefaultCharacterChunkSize * 3)]
        public async Task ProtoResponseStreamWriter_WritesDataCorrectly_ForCharactersHavingSurrogatePairs(int characterSize)
        {
            // Arrange
            // Here "𐐀" (called Deseret Long I) actually represents 2 characters. Try to make this character split across
            // the boundary
            var content = new string('a', characterSize - 1) + "𐐀";
            var stream = new TestMemoryStream();
            var writer = new ProtoResponseStreamWriter(stream, Encoding.Unicode);

            // Act
            await writer.WriteAsync(content);
            await writer.FlushAsync();

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            var streamReader = new StreamReader(stream, Encoding.Unicode);
            var actualContent = await streamReader.ReadToEndAsync();
            Assert.Equal(content, actualContent);
        }

        [Theory]
        [MemberData(nameof(ProtoResponseStreamWriterData))]
        public static void NullInputsInConstructor_ExpectArgumentNullException(Stream stream, Encoding encoding, ArrayPool<byte> bytePool, ArrayPool<char> charPool)
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var httpRequestStreamReader = new ProtoResponseStreamWriter(stream, encoding, 1, bytePool, charPool);
            });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public static void NegativeOrZeroBufferSize_ExpectArgumentOutOfRangeException(int size)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var httpRequestStreamReader = new ProtoRequestStreamReader(new MemoryStream(), Encoding.UTF8, size, ArrayPool<byte>.Shared, ArrayPool<char>.Shared);
            });
        }

        [Fact]
        public static void StreamCannotRead_ExpectArgumentException()
        {
            var mockStream = new Mock<Stream>();
            mockStream.Setup(m => m.CanWrite).Returns(false);
            Assert.Throws<ArgumentException>(() =>
            {
                var httpRequestStreamReader = new ProtoRequestStreamReader(mockStream.Object, Encoding.UTF8, 1, ArrayPool<byte>.Shared, ArrayPool<char>.Shared);
            });
        }

        [Theory]
        [MemberData(nameof(ProtoResponseDisposeData))]
        public static void StreamDisposed_ExpectedObjectDisposedException(Action<ProtoResponseStreamWriter> action)
        {
            var httpResponseStreamWriter = new ProtoResponseStreamWriter(new MemoryStream(), Encoding.UTF8, 10, ArrayPool<byte>.Shared, ArrayPool<char>.Shared);
            httpResponseStreamWriter.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                action(httpResponseStreamWriter);
            });
        }

        [Theory]
        [MemberData(nameof(ProtoResponseDisposeDataAsync))]
        public static async Task StreamDisposed_ExpectedObjectDisposedExceptionAsync(Func<ProtoResponseStreamWriter, Task> function)
        {
            var httpResponseStreamWriter = new ProtoResponseStreamWriter(new MemoryStream(), Encoding.UTF8, 10, ArrayPool<byte>.Shared, ArrayPool<char>.Shared);
            httpResponseStreamWriter.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            {
                 return function(httpResponseStreamWriter);
            });
        }


        private class TestMemoryStream : MemoryStream
        {
            public int FlushCallCount { get; private set; }

            public int FlushAsyncCallCount { get; private set; }

            public int CloseCallCount { get; private set; }

            public int DisposeCallCount { get; private set; }

            public int WriteCallCount { get; private set; }

            public int WriteAsyncCallCount { get; private set; }

            public bool ThrowOnWrite { get; set; }

            public override void Flush()
            {
                FlushCallCount++;
                base.Flush();
            }

            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                FlushAsyncCallCount++;
                return base.FlushAsync(cancellationToken);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                WriteCallCount++;
                if (ThrowOnWrite)
                    throw new IOException("Test IOException");
                base.Write(buffer, offset, count);
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                WriteAsyncCallCount++;
                if (ThrowOnWrite)
                    throw new IOException("Test IOException");
                return base.WriteAsync(buffer, offset, count, cancellationToken);
            }

            protected override void Dispose(bool disposing)
            {
                DisposeCallCount++;
                base.Dispose(disposing);
            }
        }

        public static IEnumerable<object[]> ProtoResponseStreamWriterData()
        {
            yield return new object[] { null, Encoding.UTF8, ArrayPool<byte>.Shared, ArrayPool<char>.Shared };
            yield return new object[] { new MemoryStream(), null, ArrayPool<byte>.Shared, ArrayPool<char>.Shared };
            yield return new object[] { new MemoryStream(), Encoding.UTF8, null, ArrayPool<char>.Shared };
            yield return new object[] { new MemoryStream(), Encoding.UTF8, ArrayPool<byte>.Shared, null };
        }

        public static IEnumerable<object[]> ProtoResponseDisposeData()
        {
            yield return new object[] { new Action<ProtoResponseStreamWriter>((httpResponseStreamWriter) =>
            {
                 httpResponseStreamWriter.Write('a');
            })};
            yield return new object[] { new Action<ProtoResponseStreamWriter>((httpResponseStreamWriter) =>
            {
                 httpResponseStreamWriter.Write(new char[] { 'a', 'b' }, 0, 1);
            })};

            yield return new object[] { new Action<ProtoResponseStreamWriter>((httpResponseStreamWriter) =>
            {
                httpResponseStreamWriter.Write("hello");
            })};
            yield return new object[] { new Action<ProtoResponseStreamWriter>((httpResponseStreamWriter) =>
            {
                httpResponseStreamWriter.Flush();
            })};
        }

        public static IEnumerable<object[]> ProtoResponseDisposeDataAsync()
        {
            yield return new object[] { new Func<ProtoResponseStreamWriter, Task>(async (httpResponseStreamWriter) =>
            {
                await httpResponseStreamWriter.WriteAsync('a');
            })};
            yield return new object[] { new Func<ProtoResponseStreamWriter, Task>(async (httpResponseStreamWriter) =>
            {
                await httpResponseStreamWriter.WriteAsync(new char[] { 'a', 'b' }, 0, 1);
            })};

            yield return new object[] { new Func<ProtoResponseStreamWriter, Task>(async (httpResponseStreamWriter) =>
            {
                await httpResponseStreamWriter.WriteAsync("hello");
            })};
            yield return new object[] { new Func<ProtoResponseStreamWriter, Task>(async (httpResponseStreamWriter) =>
            {
                await httpResponseStreamWriter.FlushAsync();
            })};
        }
    }
}
