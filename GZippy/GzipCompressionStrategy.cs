using System.IO;
using System.IO.Compression;

namespace GZippy
{
    public class GzipCompressionStrategy : ICompressionStrategy
    {
        public byte[] Compress(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var zipStream = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    zipStream.Write(data, 0, data.Length);
                }
                return ms.ToArray();
            }
        }

        public byte[] Decompress(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var zipStream = new GZipStream(ms, CompressionMode.Decompress))    
            {
                //todo: read to end?
                return zipStream.ReadChunk(2048);
            }
        }
    }
}