using System.IO;
using System.IO.Compression;

namespace GZippy.Gzip
{
    public class GzipCompressionStrategy : ICompressionStrategy
    {
        /// <summary>
        /// Utilizes <see cref="GZipStream"/> to compress given chunk.
        /// </summary>
        /// <param name="data">Data to compress</param>
        /// <returns>Gzip stream in byte array</returns>
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

        /// <summary>
        ///  Utilizes <see cref="GZipStream"/> to decompress given chunk.
        /// </summary>
        /// <param name="data">Data to decompress</param>
        /// <returns>Byte array containing block of uncompressed data</returns>
        public byte[] Decompress(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var zipStream = new GZipStream(ms, CompressionMode.Decompress))    
            {                
                return zipStream.ReadAllBytes();
            }
        }

        /// <summary>
        /// Parses stream containing several gzip streams and returns the first one.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>Byte array of gzip stream data</returns>
        public byte[] ParseCompressedStream(Stream stream)
        {
            return GzipParser.GetFirstGzipStream(stream);
        }       
    }
}