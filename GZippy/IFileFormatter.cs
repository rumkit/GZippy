using System.Collections.Generic;
using System.IO;

namespace GZippy
{

    public interface IFileFormatter
    {
        byte[] ParseCompressedStream(Stream stream);
        void WriteHeader(Stream stream, IEnumerable<long> offsets);
        void ReadHeader(Stream stream);
    }
}