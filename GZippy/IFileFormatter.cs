using System.Collections.Generic;
using System.IO;

namespace GZippy
{

    public interface IFileFormatter
    {
        byte[] ParseCompressedStream(Stream stream);
        void WriteTable(Stream stream, IEnumerable<long> offsets);
        void ReadTable(Stream stream);
    }
}