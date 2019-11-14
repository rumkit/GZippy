using System;
using System.Runtime.Serialization;

namespace GZippy
{
    [Serializable]
    internal class UnsupportedFileFormatException : Exception
    {
        public UnsupportedFileFormatException()
        {
        }

        public UnsupportedFileFormatException(string message) : base(message)
        {
        }

        public UnsupportedFileFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnsupportedFileFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}