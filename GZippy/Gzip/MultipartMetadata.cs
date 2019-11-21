using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZippy.Gzip
{
    static class MultipartMetadata
    {
        /// <summary>
        /// Writes specified metadata to the end of the stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="chunkOffsets"></param>
        public static void Write(Stream stream, IEnumerable<long> chunkOffsets)
        {
            long tableOffset = stream.Position;
            foreach(var offset in chunkOffsets)
            {
                stream.Write(BitConverter.GetBytes(offset),0,sizeof(long));
            }
            stream.Write(BitConverter.GetBytes(tableOffset), 0, sizeof(long));
        }

        /// <summary>
        /// Reads metadata from stream. Stream position remains the same
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static long[] Read(Stream stream)
        {
            long startPosition = stream.Position;
            long tableIndexOffset = stream.Length - sizeof(long);
            stream.Position = tableIndexOffset;
            var tableOffset = stream.ReadInt64().Value;
            stream.Position = tableOffset;
            var chunksCount = (tableIndexOffset - tableOffset) / sizeof(long);
            var offsets = new List<long>((int)chunksCount);
            while (stream.Position < tableIndexOffset)
            {
                var offset = stream.ReadInt64().Value;
                offsets.Add(offset);
            }
            stream.Position = startPosition;
            return offsets.ToArray();
        }
    }
}
