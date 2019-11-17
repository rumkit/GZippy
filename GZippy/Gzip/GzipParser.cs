using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZippy.Gzip
{
    static class GzipParser
    {
        private const byte Id1 = 0x1F;
        private const byte Id2 = 0x8b;
        private const byte CompressionMethod = 0x08;
        private const byte Flags = 0;        

        private static readonly byte[] Header = new[] { Id1, Id2, CompressionMethod, Flags };        

        public static byte[] GetFirstGzipStream(Stream stream)
        {
            byte[] firstheader = stream.ReadChunk(4);
            if (firstheader == null)
                return null;
            if (!firstheader.SequenceEqual(Header))
                throw new UnsupportedFileFormatException("File format is not supported");           
            
            var buffer = new List<byte>(firstheader);
            do
            {
                int nextByte = stream.ReadByte();
                if (nextByte < 0)
                    return buffer.ToArray();                                                        
                buffer.Add((byte)nextByte);
            }
            while (!HasNextHeader(buffer));

            for (int i = 0; i < Header.Length; i++)
                buffer.RemoveAt(buffer.Count - 1);
            stream.Position -= Header.Length;

            return buffer.ToArray();            
        }        

        private static bool HasNextHeader(List<byte> buffer)
        {
            var bufferOffset = buffer.Count - Header.Length;
            for (int i = Header.Length - 1; i >= 0; i--)
            {
                if (Header[i] != buffer[bufferOffset + i])
                    return false;
            }
            return true;
        }      
    }
}
