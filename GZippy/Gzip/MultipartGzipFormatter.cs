using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace GZippy.Gzip
{

    public class MultipartGzipFormatter : IFileFormatter
    {
        private long[] _offsets;
        private int _currentOffset;
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

        /// <summary>
        /// Write offsets table to stream
        /// </summary>
        /// <param name="stream">stream to write the table to</param>
        /// <param name="offsets">chunkOffsets</param>
        public void WriteHeader(Stream stream, IEnumerable<long> offsets)
        {
            OffsetsTable.Write(stream, offsets);
        }
        
        /// <summary>
        /// Reads header from archive
        /// </summary>
        /// <param name="stream">archive data stream</param>
        public void ReadHeader(Stream stream)
        {
            _currentOffset = 0;
            _offsets = OffsetsTable.Read(stream);
        }
    }
}