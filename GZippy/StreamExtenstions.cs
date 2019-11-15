using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZippy
{
    static class StreamExtenstions
    {
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

        public static byte[] ReadAllBytes(this Stream stream, int bufferSize=1024)
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
    }
}
