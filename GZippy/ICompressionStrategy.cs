using System.Collections.Generic;
using System.IO;

namespace GZippy
{
    public interface ICompressionStrategy
    {
        byte[] Compress(byte[] data);
        byte[] Decompress(byte[] data);        
    }
}