using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZippy
{
    static class StreamExtensions
    {
        /// <summary>
        /// Read chunk of bytes from stream with size <= maxlength
        /// </summary>
        /// <param name="stream">source stream to read from</param>
        /// <param name="maxLength">maximum length of chunk</param>
        /// <returns>byte array with chunk data</returns>
        public static byte[] ReadChunk(this Stream stream, long maxLength)
        {
            var buffer = new byte[maxLength];
            var count = stream.Read(buffer, 0, buffer.Length);
            if (count == 0)
                return null;
            if (count == maxLength)
                return buffer;
            var ret = new byte[count];
            Buffer.BlockCopy(buffer, 0, ret, 0, ret.Length);
            return ret;
        }

        /// <summary>
        /// Reads all bytes from stream using consequent reads
        /// </summary>
        /// <param name="stream">source stream to read from</param>
        /// <param name="bufferSize">size of temporary buffer</param>
        /// <returns>array of all bytes available in stream</returns>
        public static byte[] ReadAllBytes(this Stream stream, int bufferSize=1_048_577)
        {
            var buffer = new byte[bufferSize];
            var offset = 0;
            var bytesRead = 0;
            while(bufferSize == (bytesRead = stream.Read(buffer,offset,bufferSize)))
            {
                var nextBuffer = new byte[buffer.Length + bufferSize];
                Buffer.BlockCopy(buffer, 0, nextBuffer, 0, buffer.Length);
                offset = buffer.Length;
                buffer = nextBuffer;                
            }            
            var ret = new byte[buffer.Length - (bufferSize - bytesRead)];
            Buffer.BlockCopy(buffer, 0, ret, 0, ret.Length);
            return ret;
        }

        /// <summary>
        /// Reads 64-bit integer from stream
        /// </summary>
        /// <param name="stream">source stream to read from</param>        
        /// <returns></returns>
        public static long? ReadInt64(this Stream stream)
        {
            var buffer = new byte[sizeof(long)];
            var bytesRead = stream.Read(buffer,0,buffer.Length);
            if(bytesRead < sizeof(long))
                return null;
            return BitConverter.ToInt64(buffer,0);
        }
    }
}
