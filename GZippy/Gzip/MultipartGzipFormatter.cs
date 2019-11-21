using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace GZippy.Gzip
{

    public class MultipartGzipFormatter : IFileFormatter
    {
        /// <summary>
        /// Parses stream containing several gzip streams and returns the first one.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>Byte array of gzip stream data</returns>
        public byte[] ParseCompressedStream(Stream stream)
        {
            if (_currentOffset >= _offsets.Length)
                return null;
            long blockEnd;
            if (_currentOffset == _offsets.Length - 1)
                blockEnd = stream.Length;
            else
                blockEnd = _offsets[_currentOffset + 1];
            var blockLength = blockEnd - _offsets[_currentOffset++];
            var buffer = new byte[blockLength];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        public void WriteTable(Stream stream, IEnumerable<long> offsets)
        {
            OffsetsTable.Write(stream, offsets);
        }

        private long[] _offsets;
        private int _currentOffset;
        public void ReadTable(Stream stream)
        {
            _offsets = OffsetsTable.Read(stream);
        }
    }
}