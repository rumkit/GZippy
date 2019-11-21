using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZippy.Gzip
{
    static class OffsetsTable
    {
        public static void Write(Stream stream, IEnumerable<long> chunkOffsets)
        {
            long tableOffset = stream.Position;
            stream.Position = 0;
            stream.Write(BitConverter.GetBytes(tableOffset),0,sizeof(long));
            stream.Position = tableOffset;
            foreach(var offset in chunkOffsets)
            {
                stream.Write(BitConverter.GetBytes(offset),0,sizeof(long));
            }
        }

        public static long[] Read(Stream stream)
        {
            var tableOffset = stream.ReadInt64().Value;
            var dataOffset = stream.Position;
            stream.Position = tableOffset;
            var offsets = new List<long>();
            long? offset;
            while ((offset = stream.ReadInt64()) != null)
            {
                offsets.Add(offset.Value);
            }
            stream.Position = dataOffset;
            return offsets.ToArray();
        }
    }
}
