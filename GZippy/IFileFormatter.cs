using System.Collections.Generic;
using System.IO;

namespace GZippy
{

    public interface IFileFormatter
    {
        byte[] ParseCompressedStream(Stream stream);
        void WriteMetadata(Stream stream, IEnumerable<long> offsets);
        void ReadMetadata(Stream stream);
    }
}