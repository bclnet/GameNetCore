// Copyright (c) .NET Foundation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Contoso.GameNetCore.GameSockets.Test
{
    // A duplex wrapper around a read and write stream.
    public class DuplexStream : Stream
    {
        public BufferStream ReadStream { get; }
        public BufferStream WriteStream { get; }

        public DuplexStream()
            : this(new BufferStream(), new BufferStream()) { }

        public DuplexStream(BufferStream readStream, BufferStream writeStream)
        {
            ReadStream = readStream;
            WriteStream = writeStream;
        }

        public DuplexStream CreateReverseDuplexStream() => new DuplexStream(WriteStream, ReadStream);


        #region Properties

        public override bool CanRead => ReadStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanTimeout => ReadStream.CanTimeout || WriteStream.CanTimeout;

        public override bool CanWrite => WriteStream.CanWrite;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int ReadTimeout
        {
            get => ReadStream.ReadTimeout;
            set => ReadStream.ReadTimeout = value;
        }

        public override int WriteTimeout
        {
            get => WriteStream.WriteTimeout;
            set => WriteStream.WriteTimeout = value;
        }

        #endregion Properties

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override int ReadByte() => ReadStream.ReadByte();

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => ReadStream.BeginRead(buffer, offset, count, callback, state);

        public override int EndRead(IAsyncResult asyncResult) => ReadStream.EndRead(asyncResult);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => ReadStream.ReadAsync(buffer, offset, count, cancellationToken);

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => ReadStream.CopyToAsync(destination, bufferSize, cancellationToken);

        #region Read

        public override int Read(byte[] buffer, int offset, int count) => ReadStream.Read(buffer, offset, count);

        #endregion Read

        #region Write

        public override void Write(byte[] buffer, int offset, int count) => WriteStream.Write(buffer, offset, count);
        public override void WriteByte(byte value) => WriteStream.WriteByte(value);

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => WriteStream.BeginWrite(buffer, offset, count, callback, state);

        public override void EndWrite(IAsyncResult asyncResult) => WriteStream.EndWrite(asyncResult);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => WriteStream.WriteAsync(buffer, offset, count, cancellationToken);

        public override Task FlushAsync(CancellationToken cancellationToken) => WriteStream.FlushAsync(cancellationToken);

        public override void Flush() => WriteStream.Flush();

        #endregion Write

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReadStream.Dispose();
                WriteStream.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
