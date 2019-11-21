using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZippy.Gzip
{
    class GzipParser
    {
        private const byte Id1 = 0x1F;
        private const byte Id2 = 0x8b;
        private const byte CompressionMethod = 0x08;
        private const byte Flags = 0;        
        private const int DefaultBufferLength = Program.ChunkSize;

        private static readonly byte[] Header = new[] { Id1, Id2, CompressionMethod, Flags };

        private int _bytesChecked;
        private int _searchIndex;

        /// <summary>
        /// Searches for valid gzip stream starting with current position of <see cref="stream">.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>byte array of gzip stream data</returns>
        public byte[] GetFirstGzipStream(Stream stream)
        {
            if(!stream.CanRead || !stream.CanSeek)
                throw new ArgumentException("Cannot parse this stream");
            var startPosition = stream.Position;
            byte[] firstheader = stream.ReadChunk(4);
            if (firstheader == null)
                return null;
            if (!firstheader.SequenceEqual(Header))
                throw new UnsupportedFileFormatException("File format is not supported");

            _bytesChecked = 0;
            _searchIndex = 0;
            int headerPosition;
            int bytesRead = 0;
            byte[] buffer;
            
            do
            {                
                buffer = new byte[DefaultBufferLength];
                bytesRead = stream.Read(buffer, 0, buffer.Length);                
            }
            while ((headerPosition = FindHeaderPosition(buffer)) < 0 && bytesRead == buffer.Length);

            stream.Position = startPosition;
            if(headerPosition == -1)
                return stream.ReadAllBytes();            
            var gzipChunk = new byte[_bytesChecked];
            stream.Read(gzipChunk,0,gzipChunk.Length);
            return gzipChunk;
        }
        

        private int FindHeaderPosition(byte[] buffer)
        {
            for(var i = 0; i < buffer.Length; i++)
            {
                _bytesChecked++;
                if(buffer[i] == Header[_searchIndex])
                {
                    _searchIndex++;
                    if(_searchIndex >= Header.Length)
                    {
                        return (_bytesChecked + 1) - Header.Length;
                    }
                        
                }
                else
                    _searchIndex = 0;
            }
            return -1;
        }        
    }
}
