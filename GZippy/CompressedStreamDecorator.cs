using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZippy
{
    class CompressedStreamDecorator : Stream
    {
        private const byte Id1 = 0x1F;
        private const byte Id2 = 0x8b;
        private const byte CompressionMethod = 0x08;
        private const byte Flags = 0;
        private readonly byte[] _header = new[] { Id1, Id2, CompressionMethod, Flags };
        public CompressedStreamDecorator(Stream stream)
        {
            _stream = stream;
            byte[] firstheader = _stream.ReadChunk(4);
            if (!firstheader.SequenceEqual(_header))
                throw new UnsupportedFileFormatException("File format is not supported");
            _stream.Position = 0;
        }

        private readonly Stream _stream;

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position { get => _stream.Position; set => _stream.Position = value; }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {            
            var innerBuffer = new byte[count];
            int bytesRead = _stream.Read(innerBuffer, 0, innerBuffer.Length);
            int nextHeaderPosition = FindHeader(innerBuffer,bytesRead, _header);

            if(nextHeaderPosition > 0)
            {
                Position -= bytesRead - nextHeaderPosition;
                Buffer.BlockCopy(innerBuffer, 0, buffer, offset, nextHeaderPosition);
                return nextHeaderPosition;
            }

            Buffer.BlockCopy(innerBuffer, 0, buffer, offset, bytesRead);
            return bytesRead;
        }

        private int FindHeader(byte[] buffer, int bytesRead, byte[] header)
        {           
            for (int i = header.Length; i < bytesRead - header.Length; i++)
            {
                for(int j=0; j < header.Length; j++)
                {
                    if (buffer[i + j] != header[j])
                        break;
                    if (j == header.Length - 1)
                        return i;
                }
            }
            return -1;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _stream.Dispose();
        }
    }
}
