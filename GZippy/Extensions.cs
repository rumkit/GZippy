using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZippy
{
    static class Extensions
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
    }
}
